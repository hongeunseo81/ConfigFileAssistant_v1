using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConfigFileAssistant_v1
{
    public class ImageManager
    {
        private readonly string _basePath;

        public Image ExpandImageButton { get; }
        public Image CollapseImageButton { get; }
        public Image PlusImageButton { get; }
        public Image MinusImageButton { get; }
        public Image CautionImageButton { get; }
        public Image EditImageButton { get; }
        public Image ReadImageButton { get; }
        public Image FixImageButton { get; }
        public Image BrowseImageButton { get; }
        public Image ResetImageButton { get; }
        public Image SaveAsImageButton { get; }
        public Image LogoImage { get; }
        public Image ResultFailImage { get; }
        public Image ResultSuccessImage { get; }

        public ImageManager(string basePath)
        {
            _basePath = basePath;

            ExpandImageButton = Image.FromFile(Path.Combine(_basePath, "icon/down.png"));
            CollapseImageButton = Image.FromFile(Path.Combine(_basePath, "icon/up.png"));
            PlusImageButton = Image.FromFile(Path.Combine(_basePath, "icon/plus_color.png"));
            MinusImageButton = Image.FromFile(Path.Combine(_basePath, "icon/minus_color.png"));
            CautionImageButton = Image.FromFile(Path.Combine(_basePath, "icon/caution.png"));
            EditImageButton = Image.FromFile(Path.Combine(_basePath, "icon/edit-on.png"));
            ReadImageButton = Image.FromFile(Path.Combine(_basePath, "icon/edit-off.png"));
            FixImageButton = Image.FromFile(Path.Combine(_basePath, "icon/fix.png"));
            BrowseImageButton = Image.FromFile(Path.Combine(_basePath, "icon/folder-open.png"));
            ResetImageButton = Image.FromFile(Path.Combine(_basePath, "icon/refresh.png"));
            SaveAsImageButton = Image.FromFile(Path.Combine(_basePath, "icon/save-as.png"));
            LogoImage = Image.FromFile(Path.Combine(_basePath, "icon/letter-c.png"));
            ResultFailImage = Image.FromFile(Path.Combine(_basePath, "icon/failed.png"));
            ResultSuccessImage = Image.FromFile(Path.Combine(_basePath, "icon/success.png"));
        }
    }
}
