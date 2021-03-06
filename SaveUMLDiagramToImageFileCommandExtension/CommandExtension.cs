﻿using System;
using System.ComponentModel.Composition; // for [Import], [Export]
using System.Drawing; // for Bitmap
using System.Drawing.Imaging; // for ImageFormat
using System.Linq; // for collection extensions
using System.Windows.Forms; // for SaveFileDialog
using Microsoft.VisualStudio.Modeling.Diagrams; // for Diagram
using Microsoft.VisualStudio.Modeling.ExtensionEnablement; // for IGestureExtension, ICommandExtension, ILinkedUndoContext
using Microsoft.VisualStudio.ArchitectureTools.Extensibility.Presentation; // for IDiagramContext

// for designer extension attributes
using Microsoft.VisualStudio.ArchitectureTools.Extensibility.Uml;
using Microsoft.VisualStudio.ArchitectureTools.Extensibility.Layer;
using Microsoft.VisualStudio.Uml.Diagrams;

namespace SaveUMLDiagramToImageFileCommandExtension
{
    /// <summary>
    /// Called when the user clicks the menu item.
    /// </summary>
    // Context menu command applicable to any UML diagram
    [Export(typeof(ICommandExtension))]
    [ClassDesignerExtension]
    [UseCaseDesignerExtension]
    [SequenceDesignerExtension]
    [ComponentDesignerExtension]
    [ActivityDesignerExtension]
    [LayerDesignerExtension]
    public class CommandExtension : ICommandExtension
    {
        private static WeakReference<SaveFileDialog> saveDialogReference = new WeakReference<SaveFileDialog>(null);

        [Import]
        IDiagramContext Context { get; set; }

        public void Execute(IMenuCommand command)
        {
            // Get the diagram of the underlying implementation.
            Diagram dslDiagram = Context.CurrentDiagram.GetObject<Diagram>();
            if (dslDiagram != null)
            {
                var type = dslDiagram.ModelElement.GetType();
                var model = dslDiagram.ModelElement as RootModel;
                SaveFileDialog dialog = GetSaveDialog();
                dialog.FileName = model.Name;
                string imageFileName = dialog.ShowDialog() == DialogResult.OK ? dialog.FileName : null;
                if (!string.IsNullOrEmpty(imageFileName))
                {
                    switch (dialog.FilterIndex)
                    {
                        case 1:
                        case 2:
                        case 4:
                            Bitmap bitmap = dslDiagram.CreateBitmap(
                             dslDiagram.NestedChildShapes,
                             Diagram.CreateBitmapPreference.FavorClarityOverSmallSize);
                            bitmap.Save(imageFileName, GetImageType(imageFileName));
                            break;
                        case 3:
                            Metafile metafile = dslDiagram.CreateMetafile(
                             dslDiagram.NestedChildShapes);
                            metafile.Save(imageFileName, GetImageType(imageFileName));
                            break;
                    }

                }
            }
        }

        /// <summary>
        /// Called when the user right-clicks the diagram.
        /// Set Enabled and Visible to specify the menu item status.
        /// </summary>
        /// <param name="command"></param>
        public void QueryStatus(IMenuCommand command)
        {
            command.Enabled = Context.CurrentDiagram != null
              && Context.CurrentDiagram.ChildShapes.Count() > 0;
        }

        /// <summary>
        /// Menu text.
        /// </summary>
        public string Text
        {
            get { return "Save To Image..."; }
        }

        private static SaveFileDialog GetSaveDialog()
        {
            SaveFileDialog dialog;
            if (!saveDialogReference.TryGetTarget(out dialog))
            {
                dialog = CreateSaveDialog();
                saveDialogReference.SetTarget(dialog);
            }
            return dialog;
        }

        private static SaveFileDialog CreateSaveDialog()
        {
            SaveFileDialog dialog = new SaveFileDialog
            {
                AddExtension = true,
                //DefaultExt = "image.jpg",
                Filter = "JPEG File (*.jpg)|*.jpg|Portable Network Graphic (*.png)|*.png|Enhanced Metafile (*.emf)|*.emf|Bitmap (*.bmp)|*.bmp",
                FilterIndex = 1,
                Title = "Save Diagram to Image"
            };
            return dialog;
        }

        /// <summary>
        /// Return the appropriate image type for a file extension.
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        private static ImageFormat GetImageType(string fileName)
        {
            string extension = System.IO.Path.GetExtension(fileName).ToLowerInvariant();
            ImageFormat result;
            switch (extension)
            {
                case ".jpg":
                    result = ImageFormat.Jpeg;
                    break;
                case ".emf":
                    result = ImageFormat.Emf;
                    break;
                case ".png":
                    result = ImageFormat.Png;
                    break;
                case ".bmp":
                    result = ImageFormat.Bmp;
                    break;
                default:
                    result = ImageFormat.Jpeg;
                    break;
            }
            return result;
        }
    }
}