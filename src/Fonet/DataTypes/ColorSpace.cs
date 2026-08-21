namespace Genocs.Fonet.DataTypes;

internal class ColorSpace
{
    public const int DeviceUnknown = -1;
    public const int DeviceGray = 1;
    public const int DeviceRgb = 2;
    public const int DeviceCmyk = 3;

    protected int _colorSpace;
    private byte[]? _iccProfile;
    private int _componentsCount;

    public ColorSpace(int colorSpace)
    {
        _colorSpace = colorSpace;
        _componentsCount = CalculateNumComponents();
    }

    public void SetColorSpace(int colorSpace)
    {
        _colorSpace = colorSpace;
        _componentsCount = CalculateNumComponents();
    }

    public bool HasICCProfile()
    {
        return _iccProfile != null && _iccProfile.Length > 0;
    }

    public byte[] GetICCProfile()
    {
        if (HasICCProfile())
        {
            return _iccProfile!;
        }
        else
        {
            return [];
        }
    }

    public void SetICCProfile(byte[] iccProfile)
        => _iccProfile = iccProfile;

    public int GetColorSpace()
        => _colorSpace;

    public int GetNumComponents()
        => _componentsCount;

    public string GetColorSpacePDFString()
    {
        return _colorSpace switch
        {
            DeviceGray => "DeviceGray",
            DeviceRgb => "DeviceRGB",
            DeviceCmyk => "DeviceCMYK",
            _ => "DeviceRGB",
        };
    }

    private int CalculateNumComponents()
    {
        return _colorSpace switch
        {
            DeviceGray => 1,
            DeviceRgb => 3,
            DeviceCmyk => 4,
            _ => 0,
        };
    }
}