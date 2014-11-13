using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Reflection;
using System.Security;
using System.Security.Permissions;
using System.Security.Policy;
using System.Runtime.Remoting;
using System.Collections;
using System.Runtime.InteropServices;

using mscoree; // for domain enum. 
//Add the following as a COM reference - C:\WINDOWS\Microsoft.NET\Framework|Framework64\vXXXXXX\mscoree.tlb

namespace Pumpkin {

    public sealed class SnippetLoader : MarshalByRefObject {

        static Assembly domain_AssemblyResolve(object sender, ResolveEventArgs args) {

            // TODO: Here we have a list of the assemblies we allow in snippets.
            // They may be stored in a local dir, or in a DB. Locate them, deserialize (if needed)
            // and load

            var monitorAssembly = typeof(Pumpkin.Monitor).Assembly;
            if (args.Name.Equals(monitorAssembly.FullName)) {
                
                // NOTE: if you want to load it from DB or file, you will need to asser permission here
                // or the load will fail (no permissions in this sandbox). At least, you need System.Security.Permissions and 
                // System.Security.Permissions.FileIOPermission
                // Assembly.LoadFrom(monitorAssembly.Location);
                return monitorAssembly;
            }

            return null;
        }

        public SnippetResult Run(byte[] compiledAssembly, string className) {
            var snippetAssembly = Assembly.Load(compiledAssembly, null, SecurityContextSource.CurrentAppDomain);

            // The add_AssemblyResolve is SecurityCritical. We need to assert permission (from here, a trusted 
            // assembly) to make this run
            new ReflectionPermission(PermissionState.Unrestricted).Assert();
            AppDomain.CurrentDomain.AssemblyResolve += domain_AssemblyResolve;
            ReflectionPermission.RevertAssert();

            var monitor = new Monitor();

            //Load the MethodInfo for a method in the new Assembly. This might be a method you know, or 
            //you can use Assembly.EntryPoint to get to the main function in an executable.
            MethodInfo target = snippetAssembly.GetType(className).GetMethod("SnippetMain");
            try {
                //Now invoke the method.
                target.Invoke(null, new object[] { monitor });
                //Alternativaly: snippetAssembly.EntryPoint.Invoke(null, null);  

                return new SnippetResult(monitor.output);
            }
            catch (Exception ex) {
                // When we print informations from a SecurityException extra information can be printed if we are 
                //calling it with a full-trust stack.
                (new PermissionSet(PermissionState.Unrestricted)).Assert();
                System.Diagnostics.Debug.WriteLine("Exception caught:\n{0}", ex.ToString());
                CodeAccessPermission.RevertAssert();

                return new SnippetResult(ex);
            }            
        }
    }


    public class SnippetRunner: MarshalByRefObject {

        private static AppDomain CreateSandbox(string sandboxName) {
            AppDomain domain = GetAppDomains().Where(x => x.FriendlyName == sandboxName).FirstOrDefault();

            if (domain == null) {

                //Setting the AppDomainSetup. It is very important to set the ApplicationBase to a folder 
                //other than the one in which the sandboxer resides.
                AppDomainSetup adSetup = new AppDomainSetup();
                adSetup.ApplicationBase = "NOT_A_PATH";

                //Setting the permissions for the AppDomain. We give the permission to execute and to 
                //read/discover the location where the untrusted code is loaded.
                PermissionSet permSet = new PermissionSet(PermissionState.None);
                permSet.AddPermission(new SecurityPermission(SecurityPermissionFlag.Execution));

                //We want the sandboxer assembly's strong name, so that we can add it to the full trust list.
                StrongName fullTrustAssembly = typeof(SnippetRunner).Assembly.Evidence.GetHostEvidence<StrongName>();

                //Now we have everything we need to create the AppDomain, so let's create it.
                domain = AppDomain.CreateDomain(sandboxName, null, adSetup, permSet, fullTrustAssembly);
            }

            return domain;
        }        

        // TODO: replace this with a method in our host/appdomainmanager pair
        public static IList<AppDomain> GetAppDomains() {
            IList<AppDomain> appDomains = new List<AppDomain>();
            IntPtr enumHandle = IntPtr.Zero;
            CorRuntimeHost host = new CorRuntimeHost();
            try {
                host.EnumDomains(out enumHandle);
                object domain = null;
                while (true) {
                    host.NextDomain(enumHandle, out domain);
                    if (domain == null) break;
                    AppDomain appDomain = (AppDomain)domain;
                    appDomains.Add(appDomain);
                }
                return appDomains;
            }
            catch (Exception e) {
                System.Diagnostics.Debug.WriteLine(e.ToString());
                return null;
            }
            finally {
                host.CloseEnum(enumHandle);
                Marshal.ReleaseComObject(host);
            }
        }

        public static SnippetResult Run(byte[] assembly, string className) {

            AppDomain sandboxDomain = CreateSandbox("SnippetSandbox");

            //Use CreateInstanceFrom to load an instance of the Sandboxer class into the
            //new AppDomain. 
            ObjectHandle handle = Activator.CreateInstanceFrom(
                sandboxDomain, typeof(SnippetLoader).Assembly.ManifestModule.FullyQualifiedName,
                typeof(SnippetLoader).FullName);

            //Unwrap the instance created in the new domain into a reference in this domain and use it to execute the 
            //untrusted code.
            SnippetLoader loader = (SnippetLoader)handle.Unwrap();
            return loader.Run(assembly, className);
        }
    }
}
