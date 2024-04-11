using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Win32.SafeHandles;

namespace WinCredentialManager
{
    /// <summary>
    /// Enum for Credential Type. This program currently only supports Generic credentials.
    /// </summary>
    public enum CredentialType
    {
        Generic = 1,
    }

    /// <summary>
    /// A Class for storing the credential information from Windows Credential Manager.
    /// <para>Contains the <see cref="CredentialType"/>, ApplicationName, UserName, and Password of the generic credential.</para>
    /// </summary>
    public class Credential
    {
        public CredentialType CredentialType { get; }
        public string ApplicationName { get; }
        public string UserName { get; }
        public string Password { get; }

        public Credential(CredentialType credentialType, string applicationName, string userName, string password)
        {
            CredentialType = credentialType;
            ApplicationName = applicationName;
            UserName = userName;
            Password = password;
        }
    }

    /// <summary>
    /// A Class for interacting with Windows Credential Manager via P/Invoke.
    /// </summary>
    public static class CredentialManager
    {
        [DllImport("Advapi32.dll", EntryPoint = "CredDeleteW", CharSet = CharSet.Unicode, SetLastError = true)]
        static extern bool CredDelete(string target, CredentialType type, int reservedFlag);

        [DllImport("Advapi32.dll", EntryPoint = "CredFree", SetLastError = true)]
        static extern bool CredFree([In] nint cred);

        [DllImport("Advapi32.dll", EntryPoint = "CredReadW", CharSet = CharSet.Unicode, SetLastError = true)]
        static extern bool CredRead(string target, CredentialType type, int reservedFlag, out nint credentialPtr);

        [DllImport("Advapi32.dll", EntryPoint = "CredWriteW", CharSet = CharSet.Unicode, SetLastError = true)]
        static extern bool CredWrite([In] ref CREDENTIAL userCredential, [In] uint flags);

        /// <summary>
        /// The CREDENTIAL struct from the Windows Credential Manager API.
        /// </summary>
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct CREDENTIAL
        {
            public uint Flags;
            public CredentialType Type;
            public nint TargetName;
            public nint Comment;
            public System.Runtime.InteropServices.ComTypes.FILETIME LastWritten;
            public uint CredentialBlobSize;
            public nint CredentialBlob;
            public uint Persist;
            public uint AttributeCount;
            public nint Attributes;
            public nint TargetAlias;
            public nint UserName;
        }

        /// <summary>
        /// Deletes a credential from Windows Credential Manager.
        /// </summary>
        /// <exception cref="Exception">Returns the error from CredDelete</exception>
        public static void DeleteCredential(string applicationName)
        {
            if (!CredDelete(applicationName, CredentialType.Generic, 0))
            {
                int lastError = Marshal.GetLastWin32Error();
                throw new Exception($"CredDelete failed with the error code {lastError}.");
            }
        }

        /// <summary>
        /// Marshalls the CREDENTIAL struct into a Credential object.
        /// </summary>
        /// <param name="credential">The CREDENTIAL struct</param>
        /// <returns><see cref="Credential"/></returns>
        private static Credential MarshalCredential (CREDENTIAL credential)
        {
            string ?applicationName = Marshal.PtrToStringUni(credential.TargetName);
            string ?userName = Marshal.PtrToStringUni(credential.UserName);
            string ?secret = null;
            if (credential.CredentialBlob != nint.Zero)
            {
                secret = Marshal.PtrToStringUni(credential.CredentialBlob, (int)credential.CredentialBlobSize / 2);
            }
            if (applicationName == null || userName == null || secret == null)
            {
                throw new InvalidOperationException("Failed to Read credential from Windows Credential Manager");
            }
            return new Credential(credential.Type, applicationName, userName, secret);
        }

        /// <summary>
        /// Read a credential from Windows Credential Manager.
        /// </summary>
        public static Credential? ReadCredential(string applicationName)
        {
            nint nCredPtr;
            bool read = CredRead(applicationName, CredentialType.Generic, 0, out nCredPtr);
            if (read)
            {
                using (CriticalCredentialHandle critCred = new CriticalCredentialHandle(nCredPtr))
                {
                    CREDENTIAL cred = critCred.GetCredential();
                    return MarshalCredential(cred);
                }
            }
            return null;
        }

        /// <summary>
        /// Writes a credential to Windows Credential Manager.
        /// </summary>
        /// <exception cref="Exception">Returns the error from CredWrite</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the secret message exceeds 2560 bytes.</exception>
        public static void WriteCredential(string applicationName, string userName, string secret)
        {
            byte[] byteArray = Encoding.Unicode.GetBytes(secret);
            if (byteArray.Length > 512 * 5) 
            { 
                throw new ArgumentOutOfRangeException("secret", "The secret message has exceeded 2560 bytes.");
            };

            CREDENTIAL credential = new CREDENTIAL();
            credential.AttributeCount = 0;
            credential.Attributes = nint.Zero;
            credential.Comment = nint.Zero;
            credential.CredentialBlob = Marshal.StringToCoTaskMemUni(secret);
            credential.CredentialBlobSize = (uint)(byteArray == null ? 0 : byteArray.Length);
            credential.Persist = 2; // Represents LocalMachine Persistence
            credential.TargetAlias = nint.Zero;
            credential.TargetName = Marshal.StringToCoTaskMemUni(applicationName);
            credential.Type = CredentialType.Generic;
            credential.UserName = Marshal.StringToCoTaskMemUni(userName ?? Environment.UserName);

            bool written = CredWrite(ref credential, 0);
            Marshal.FreeCoTaskMem(credential.CredentialBlob);
            Marshal.FreeCoTaskMem(credential.TargetName);
            Marshal.FreeCoTaskMem(credential.UserName);

            if (!written)
            {
                int lastError = Marshal.GetLastWin32Error();
                throw new Exception(string.Format("CredWrite failed with the error code {0}.", lastError));
            }
        }

        /// <summary>
        /// CriticalHandle for the CREDENTIAL struct.
        /// <para>See docs: 
        /// <see cref="https://learn.microsoft.com/en-us/dotnet/api/system.runtime.interopservices.criticalhandle?view=net-8.0"/>
        /// </para>    
        /// </summary>
        sealed class CriticalCredentialHandle : CriticalHandleZeroOrMinusOneIsInvalid
        {
            public CriticalCredentialHandle(nint preexistingHandle)
            {
                SetHandle(preexistingHandle);
            }

            /// <summary>
            /// Returns the CREDENTIAL struct from the handle.
            /// </summary>
            /// <exception cref="InvalidOperationException"></exception>
            public CREDENTIAL GetCredential()
            {
                if (!IsInvalid)
                {
                    #pragma warning disable CS8605 // Unboxing a possibly null value.
                    CREDENTIAL credential = (CREDENTIAL)Marshal.PtrToStructure(handle, typeof(CREDENTIAL));
                    #pragma warning restore CS8605 // Unboxing a possibly null value.
                    return credential;
                }

                throw new InvalidOperationException("Invalid CriticalHandle!");
            }

            /// <summary>
            /// Release the handle and use CredFree to free the memory to avoid memory leaks.
            /// </summary>
            /// <returns></returns>
            protected override bool ReleaseHandle()
            {
                if (!IsInvalid)
                {
                    CredFree(handle);
                    SetHandleAsInvalid();
                    return true;
                }

                return false;
            }
        }
    }
}
