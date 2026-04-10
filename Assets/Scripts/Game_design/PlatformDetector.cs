using YG;

public enum Platform
{
    Desktop,
    Mobile,
	Tablet
}

public static class PlatformDetector
{ 
    public static Platform GetPlatform()
    {
        if (YG2.envir.isDesktop)
            return Platform.Desktop;
        else if (YG2.envir.isTablet)
            return Platform.Tablet;
        else if (YG2.envir.isMobile)
            return Platform.Mobile;
        else
            return Platform.Desktop;
    }
}
