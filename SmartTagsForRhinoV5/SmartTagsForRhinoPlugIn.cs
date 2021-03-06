﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SmartTagsForRhino.Core;

namespace SmartTagsForRhino
{
    ///<summary>
    /// <para>Every RhinoCommon .rhp assembly must have one and only one PlugIn-derived
    /// class. DO NOT create instances of this class yourself. It is the
    /// responsibility of Rhino to create an instance of this class.</para>
    /// <para>To complete plug-in information, please also see all PlugInDescription
    /// attributes in AssemblyInfo.cs (you might need to click "Project" ->
    /// "Show All Files" to see it in the "Solution Explorer" window).</para>
    ///</summary>
    public class SmartTagsForRhinoPlugIn : Rhino.PlugIns.PlugIn

    {
        public SmartTagsForRhinoPlugIn()
        {
            Instance = this;
        }

        ///<summary>Gets the only instance of the smartTagsPlugIn plug-in.</summary>
        public static SmartTagsForRhinoPlugIn Instance
        {
            get; private set;
        }

        /// <summary>
        /// Override OnLoad to register my custom panel type with Rhino when my plug-in
        /// is loaded.
        /// </summary>
        /// <param name="errorMessage"></param>
        /// <returns></returns>
        protected override Rhino.PlugIns.LoadReturnCode OnLoad(ref string errorMessage)
        {
            // Get MyPanel class type
            System.Type panelType = typeof(Panel_TagManager);
            // Register my custom panel class type with Rhino, the custom panel my be display
            // by running the MyOpenPanel command and hidden by running the MyClosePanel command.
            // You can also include the custom panel in any existing panel group by simply right
            // clicking one a panel tab and checking or un-checking the "MyPane" option.
            System.Drawing.Icon icon = Properties.Resources.TagIcon;
            Rhino.UI.Panels.RegisterPanel(this, panelType, "TagManager", icon);

            Rhino.RhinoDoc.SelectObjects += UpdateTagsForSelectEvent;
            Rhino.RhinoDoc.DeselectAllObjects += RhinoDoc_DeselectAllObjects;
            Rhino.RhinoDoc.DeselectObjects += UpdateTagsForDeselectEvent;

            Rhino.RhinoDoc.EndOpenDocument += RhinoDoc_EndOpenDocument;

            return Rhino.PlugIns.LoadReturnCode.Success;
        }

        private void RhinoDoc_EndOpenDocument(object sender, Rhino.DocumentOpenEventArgs e)
        {
            TagUtil.ActiveDocument = e.Document;
            TagUtil.SetCurrentDocumentTags(TagUtil.GetAllTags(e.Document), e.Merge);
            TagUtil.LoadSavedFiltersFromDocument();
        }

        protected override void OnShutdown()
        {
            Rhino.RhinoDoc.SelectObjects -= UpdateTagsForSelectEvent;
            Rhino.RhinoDoc.DeselectAllObjects -= RhinoDoc_DeselectAllObjects;
            Rhino.RhinoDoc.DeselectObjects -= UpdateTagsForDeselectEvent;
            base.OnShutdown();
        }

        private void RhinoDoc_DeselectAllObjects(object sender, Rhino.DocObjects.RhinoDeselectAllObjectsEventArgs e)
        {
            TagUtil.ActiveDocument = e.Document;
            TagUtil.TagManager?.UpdateAllObjectsDeselected(true);
        }

        private void UpdateTagsForDeselectEvent(object sender, Rhino.DocObjects.RhinoObjectSelectionEventArgs e)
        {
            TagUtil.ActiveDocument = e.Document;
            //update the tag manager UI to reflect the selection
            if (e.Selected) { return; }
            Dictionary<string, List<Guid>> tags = TagUtil.GetTagUidMap(e.RhinoObjects);
            TagUtil.TagManager?.UpdateSelectedObjectTags(tags, e.Selected, true);
            TagUtil.TagManager?.RemoveFromSelectedObjects(e.RhinoObjects.Select(obj => obj.Id));
        }

        private void UpdateTagsForSelectEvent(object sender, Rhino.DocObjects.RhinoObjectSelectionEventArgs e)
        {
            TagUtil.ActiveDocument = e.Document;
            //update the tag manager UI to reflect the selection
            if (!e.Selected) { return; }
            Dictionary<string, List<Guid>> tags = TagUtil.GetTagUidMap(e.RhinoObjects);
            TagUtil.TagManager?.UpdateSelectedObjectTags(tags, e.Selected, true);
            TagUtil.TagManager?.AddToSelectedObjects(e.RhinoObjects.Select(obj => obj.Id));
        }

        // You can override methods here to change the plug-in behavior on
        // loading and shut down, add options pages to the Rhino _Option command
        // and mantain plug-in wide options in a document.
    }
}