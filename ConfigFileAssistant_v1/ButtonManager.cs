using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ConfigFileAssistant_v1
{
    public class ButtonImageManager
    {
        private readonly ImageManager _imageManager;
        private readonly ToolTip _toolTip;

        public ButtonImageManager(ImageManager imageManager, ToolTip toolTip)
        {
            _imageManager = imageManager;
            _toolTip = toolTip;
        }

        public void SetupButtonImages(Button expandAllButton, Button modeButton, Button fileBrowseButton, Button codeBrowseButton, Button backupBrowseButton, Button fixAllButton, Button saveAsButton, Button resetButton)
        {
            var resultImageList = new ImageList { ImageSize = new Size(200, 200) };
            resultImageList.Images.Add("success", _imageManager.ResultSuccessImage);
            resultImageList.Images.Add("fail", _imageManager.ResultFailImage);

            var expandImageList = new ImageList { ImageSize = new Size(32, 32) };
            expandImageList.Images.Add("collapse", _imageManager.CollapseImageButton);
            expandImageList.Images.Add("expand", _imageManager.ExpandImageButton);

            var editModeImageList = new ImageList { ImageSize = new Size(32, 32) };
            editModeImageList.Images.Add("edit-off", _imageManager.ReadImageButton);
            editModeImageList.Images.Add("edit-on", _imageManager.EditImageButton);

            var commandImageList = new ImageList { ImageSize = new Size(16, 16) };
            commandImageList.Images.Add("plus", _imageManager.PlusImageButton);
            commandImageList.Images.Add("minus", _imageManager.MinusImageButton);
            commandImageList.Images.Add("caution", _imageManager.CautionImageButton);

            var buttonImageList = new ImageList { ImageSize = new Size(25, 25) };
            buttonImageList.Images.Add("browse", _imageManager.BrowseImageButton);
            buttonImageList.Images.Add("fix", _imageManager.FixImageButton);
            buttonImageList.Images.Add("save-as", _imageManager.SaveAsImageButton);
            buttonImageList.Images.Add("reset", _imageManager.ResetImageButton);

            expandAllButton.ImageList = expandImageList;
            expandAllButton.Image = expandImageList.Images["collapse"];
            RemoveButtonBorder(expandAllButton);

            modeButton.ImageList = editModeImageList;
            modeButton.Image = editModeImageList.Images["edit-off"];
            _toolTip.SetToolTip(modeButton, "Current View: Read-Only, Click to Edit");
            RemoveButtonBorder(modeButton);

            fileBrowseButton.ImageList = buttonImageList;
            fileBrowseButton.Image = buttonImageList.Images["browse"];
            _toolTip.SetToolTip(fileBrowseButton, "Browse other config files");
            RemoveButtonBorder(fileBrowseButton);

            codeBrowseButton.ImageList = buttonImageList;
            codeBrowseButton.Image = buttonImageList.Images["browse"];
            _toolTip.SetToolTip(codeBrowseButton, "Browse other code files");
            RemoveButtonBorder(codeBrowseButton);

            backupBrowseButton.ImageList = buttonImageList;
            backupBrowseButton.Image = buttonImageList.Images["browse"];
            _toolTip.SetToolTip(backupBrowseButton, "Change backup file path");
            RemoveButtonBorder(backupBrowseButton);

            fixAllButton.ImageList = buttonImageList;
            fixAllButton.Image = buttonImageList.Images["fix"];
            _toolTip.SetToolTip(fixAllButton, "Fix All Errors");

            saveAsButton.ImageList = buttonImageList;
            saveAsButton.Image = buttonImageList.Images["save-as"];
            _toolTip.SetToolTip(saveAsButton, "Save As");

            resetButton.ImageList = buttonImageList;
            resetButton.Image = buttonImageList.Images["reset"];
            _toolTip.SetToolTip(resetButton, "Reset");
        }

        private void RemoveButtonBorder(Button button)
        {
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderColor = Color.FromArgb(0, 255, 255, 255);
            button.FlatAppearance.BorderSize = 0;
        }
    }

}
