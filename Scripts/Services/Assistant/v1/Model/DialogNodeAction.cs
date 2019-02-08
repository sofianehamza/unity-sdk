/**
* Copyright 2019 IBM Corp. All Rights Reserved.
*
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
*
*      http://www.apache.org/licenses/LICENSE-2.0
*
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
*
*/

using FullSerializer;

namespace IBM.Watson.Assistant.v1.Model
{
    /// <summary>
    /// DialogNodeAction.
    /// </summary>
    public class DialogNodeAction
    {
        /// <summary>
        /// The type of action to invoke.
        /// </summary>
        public class ActionTypeEnumValue
        {
            /// <summary>
            /// Constant CLIENT for client
            /// </summary>
            public const string CLIENT = "client";
            /// <summary>
            /// Constant SERVER for server
            /// </summary>
            public const string SERVER = "server";
            /// <summary>
            /// Constant CLOUD_FUNCTION for cloud_function
            /// </summary>
            public const string CLOUD_FUNCTION = "cloud_function";
            /// <summary>
            /// Constant WEB_ACTION for web_action
            /// </summary>
            public const string WEB_ACTION = "web_action";
            
        }

        /// <summary>
        /// The type of action to invoke.
        /// </summary>
        [fsProperty("type")]
        public string ActionType { get; set; }
        /// <summary>
        /// The name of the action.
        /// </summary>
        [fsProperty("name")]
        public string Name { get; set; }
        /// <summary>
        /// A map of key/value pairs to be provided to the action.
        /// </summary>
        [fsProperty("parameters")]
        public object Parameters { get; set; }
        /// <summary>
        /// The location in the dialog context where the result of the action is stored.
        /// </summary>
        [fsProperty("result_variable")]
        public string ResultVariable { get; set; }
        /// <summary>
        /// The name of the context variable that the client application will use to pass in credentials for the action.
        /// </summary>
        [fsProperty("credentials")]
        public string Credentials { get; set; }
    }


}