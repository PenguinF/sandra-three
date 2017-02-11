/*********************************************************************************
 * UIActionHandler.cs
 * 
 * Copyright (c) 2004-2017 Henk Nicolai
 * 
 *    Licensed under the Apache License, Version 2.0 (the "License");
 *    you may not use this file except in compliance with the License.
 *    You may obtain a copy of the License at
 * 
 *        http://www.apache.org/licenses/LICENSE-2.0
 * 
 *    Unless required by applicable law or agreed to in writing, software
 *    distributed under the License is distributed on an "AS IS" BASIS,
 *    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 *    See the License for the specific language governing permissions and
 *    limitations under the License.
 * 
 *********************************************************************************/
namespace Sandra.UI.WF
{
    /// <summary>
    /// Delegate which verifies if an action can be performed, and optionally performs it.
    /// </summary>
    /// <param name="perform">
    /// Whether or not to actually perform the action.
    /// </param>
    /// <returns>
    /// A complete <see cref="UIActionState"/> if <paramref name="perform"/> is false,
    /// or a <see cref="UIActionState"/> indicating whether or not the action was performed successfully, if <paramref name="perform"/> is true.
    /// </returns>
    public delegate UIActionState UIActionHandlerFunc(bool perform);

    /// <summary>
    /// Responsible for managing a set of <see cref="UIAction"/>s and their associated handlers.
    /// </summary>
    public class UIActionHandler
    {
    }

    /// <summary>
    /// Interface implemented by <see cref="System.Windows.Forms.Control"/> subclasses to hook into the <see cref="UIAction"/> framework.
    /// </summary>
    public interface IUIActionHandlerProvider
    {
        UIActionHandler ActionHandler { get; }
    }
}
