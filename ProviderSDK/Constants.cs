// 
//  Copyright (c) Microsoft Corporation. All rights reserved. 
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at
//  http://www.apache.org/licenses/LICENSE-2.0
//  
//  Unless required by applicable law or agreed to in writing, software
//  distributed under the License is distributed on an "AS IS" BASIS,
//  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  See the License for the specific language governing permissions and
//  limitations under the License.
//  

namespace OneGet.ProviderSDK {
    public static class Constants {
        #region copy common-constants-implementation ** public

        public const string MinVersion = "0.0.0.1";
        public static string[] Empty = new string[0];
        public const string MSGPrefix = "MSG:";

        public static class PackageStatus {
            public const string Installed = "Installed";
            public const string Uninstalled = "Uninstalled";
        }

        public static class SwidTag {
            public const string SoftwareIdentity = "SoftwareIdentity";
        }

        public static class Features {
            public const string AutomationOnly = "automation-only";
        }

        public static class Parameters {
            public const string IsUpdate = "IsUpdatePackageSource";
            public const string Name = "Name";
            public const string Location = "Location";
        }

        public static class Messages {
            public const string UnableToDownload = "MSG:UnableToDownload";
            public const string FailedProviderBootstrap = "MSG:FailedProviderBootstrap";
            public const string UnknownProvider = "MSG:UnknownProvider";
            public const string UserDeclinedUntrustedPackageInstall = "MSG:UserDeclinedUntrustedPackageInstall";
            public const string ProviderPluginLoadFailure = "MSG:ProviderPluginLoadFailure";
            public const string ProviderSwidtagUnavailable = "MSG:ProviderSwidtagUnavailable";
            public const string UnableToResolvePackage = "MSG:UnableToResolvePackage";
            public const string UnsupportedProviderType = "MSG:UnsupportedProviderType";
            public const string DestinationPathNotSet = "MSG:DestinationPathNotSet";
            public const string InvalidFilename = "MSG:InvalidFilename";
            public const string UnableToRemoveFile = "MSG:UnableToRemoveFile";
            public const string FileFailedVerification = "MSG:FileFailedVerification";
            public const string MissingRequiredParameter = "MSG:MissingRequiredParameter";
            public const string SchemeNotSupported = "MSG:SchemeNotSupported";
            public const string PackageProviderExists = "MSG:PackageProviderExists";
            public const string UnableToResolveSource = "MSG:UnableToResolveSource_NameOrLocation";
            public const string UnsupportedArchive = "MSG:UnsupportedArchive";
            public const string CreatefolderFailed = "MSG:CreatefolderFailed";
            public const string UnableToOverwriteExistingFile = "MSG:UnableToOverwriteExistingFile";
            public const string UnableToCopyFileTo = "MSG:UnableToCopyFileTo";
            public const string UnableToCreateShortcutTargetDoesNotExist = "MSG:UnableToCreateShortcutTargetDoesNotExist";
            public const string RemoveEnvironmentVariableRequiresElevation = "MSG:RemoveEnvironmentVariableRequiresElevation";
            public const string UnknownFolderId = "MSG:UnknownFolderId";
            public const string ProtocolNotSupported = "MSG:ProtocolNotSupported";

            // NEW:
            public const string SourceLocationNotValid = "MSG:SourceLocationNotValid_Location";
            public const string UriSchemeNotSupported = "MSG:UriSchemeNotSupported_Scheme";
            public const string PackageFailedInstall = "MSG:UnableToInstallPackage_package_reason";
            public const string DependencyResolutionError = "MSG:UnableToResolveDependency_dependencyPackage";
            public const string DependentPackageFailedInstall = "MSG:DependentPackageFailedInstall_dependencyPackage";

        }

        public static class OptionType {
            public const string String = "String";
            public const string StringArray = "StringArray";
            public const string Int = "Int";
            public const string Switch = "Switch";
            public const string Folder = "Folder";
            public const string File = "File";
            public const string Path = "Path";
            public const string Uri = "Uri";
            public const string SecureString = "SecureString";
        }

        #endregion

        /*
        #region declare constants-implementation

        public const string MSGPrefix = "MSG:";
        public const string TerminatingError = "MSG:TerminatingError";
        public const string SourceLocationNotValid = "MSG:SourceLocationNotValid_Location";
        public const string UriSchemeNotSupported = "MSG:UriSchemeNotSupported_Scheme";
        public const string UnableToResolveSource = "MSG:UnableToResolveSource_NameOrLocation";
        public const string PackageFailedInstall = "MSG:UnableToInstallPackage_package_reason";
        public const string DependencyResolutionError = "MSG:UnableToResolveDependency_dependencyPackage";
        public const string DependentPackageFailedInstall = "MSG:DependentPackageFailedInstall_dependencyPackage";
        public const string PackageProviderExists = "MSG:PackageProviderExists";
        public const string MissingRequiredParameter = "MSG:MissingRequiredParameter";

        public const string IsUpdateParameter = "IsUpdatePackageSource";

        public const string NameParameter = "Name";
        public const string LocationParameter = "Location";
        
        #endregion
        */
    }
}