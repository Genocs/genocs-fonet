namespace Genocs.Fonet.Fo;

internal struct Status(int code)
{
    private readonly int _code = code;

    public const int OK = 1;
    public const int AREA_FULL_NONE = 2;
    public const int AREA_FULL_SOME = 3;
    public const int FORCE_PAGE_BREAK = 4;
    public const int FORCE_PAGE_BREAK_EVEN = 5;
    public const int FORCE_PAGE_BREAK_ODD = 6;
    public const int FORCE_COLUMN_BREAK = 7;
    public const int KEEP_WITH_NEXT = 8;

    public int GetCode()
        => _code;

    public bool IsIncomplete()
        => (_code != OK) && (_code != KEEP_WITH_NEXT);

    public readonly bool LaidOutNone()
        => _code == AREA_FULL_NONE;

    public readonly bool IsPageBreak()
        => (_code == FORCE_PAGE_BREAK) || (_code == FORCE_PAGE_BREAK_EVEN) || (_code == FORCE_PAGE_BREAK_ODD);
}