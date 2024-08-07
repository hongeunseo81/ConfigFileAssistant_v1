using System.Drawing;
using System.IO;
using System.Windows.Forms;

public class ImageManager
{
    private readonly string _iconBasePath;
    public ImageList CommandImageList { get; private set; }
    public ImageList ExpandImageList { get; private set; }
    public ImageList EditModeImageList { get; private set; }
    public ImageList ButtonImageList { get; private set; }
    public ImageList ResultImageList { get; private set; }

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
    public Icon LogoIcon { get; }
    public Image ResultFailImage { get; }
    public Image ResultSuccessImage { get; }

    public ImageManager(string basePath)
    {
        _iconBasePath = Path.Combine(basePath, "icon");

        ExpandImageButton = Image.FromFile(Path.Combine(_iconBasePath, "down.png"));
        CollapseImageButton = Image.FromFile(Path.Combine(_iconBasePath, "up.png"));
        PlusImageButton = Image.FromFile(Path.Combine(_iconBasePath, "plus_color.png"));
        MinusImageButton = Image.FromFile(Path.Combine(_iconBasePath, "minus_color.png"));
        CautionImageButton = Image.FromFile(Path.Combine(_iconBasePath, "caution.png"));
        EditImageButton = Image.FromFile(Path.Combine(_iconBasePath, "edit-on.png"));
        ReadImageButton = Image.FromFile(Path.Combine(_iconBasePath, "edit-off.png"));
        FixImageButton = Image.FromFile(Path.Combine(_iconBasePath, "fix.png"));
        BrowseImageButton = Image.FromFile(Path.Combine(_iconBasePath, "folder-open.png"));
        ResetImageButton = Image.FromFile(Path.Combine(_iconBasePath, "refresh.png"));
        SaveAsImageButton = Image.FromFile(Path.Combine(_iconBasePath, "save-as.png"));
        LogoImage = Image.FromFile(Path.Combine(_iconBasePath, "letter-c.png"));
        LogoIcon = new Icon(Path.Combine(Path.Combine("icon/letter-c.ico")));
        ResultFailImage = Image.FromFile(Path.Combine(_iconBasePath, "failed.png"));
        ResultSuccessImage = Image.FromFile(Path.Combine(_iconBasePath, "success.png"));

        InitializeImageLists();
    }

    private void InitializeImageLists()
    {
        ResultImageList = new ImageList { ImageSize = new Size(200, 200) };
        ResultImageList.Images.Add(ResultSuccessImage);
        ResultImageList.Images.Add(ResultFailImage);

        ExpandImageList = new ImageList { ImageSize = new Size(32, 32) };
        ExpandImageList.Images.Add("collapse", CollapseImageButton);
        ExpandImageList.Images.Add("expand", ExpandImageButton);

        EditModeImageList = new ImageList { ImageSize = new Size(32, 32) };
        EditModeImageList.Images.Add("edit-off", ReadImageButton);
        EditModeImageList.Images.Add("edit-on", EditImageButton);

        CommandImageList = new ImageList { ImageSize = new Size(16, 16) };
        CommandImageList.Images.Add("plus", PlusImageButton);
        CommandImageList.Images.Add("minus", MinusImageButton);
        CommandImageList.Images.Add("caution", CautionImageButton);

        ButtonImageList = new ImageList { ImageSize = new Size(25, 25) };
        ButtonImageList.Images.Add("browse", BrowseImageButton);
        ButtonImageList.Images.Add("fix", FixImageButton);
        ButtonImageList.Images.Add("save-as", SaveAsImageButton);
        ButtonImageList.Images.Add("reset", ResetImageButton);
    }

    public void SetButtonImage(Button button, string imageName, ImageList imageList, bool hasBoarder = true)
    {
        button.ImageList = imageList;
        button.Image = imageList.Images[imageName];
        if(!hasBoarder)
        {
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderColor = Color.FromArgb(0, 255, 255, 255);
            button.FlatAppearance.BorderSize = 0;
        }
    }
}
