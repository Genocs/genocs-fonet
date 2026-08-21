namespace Genocs.Fonet.Pdf.Filter;

public class UnsupportedFilterException(string filterName) : Exception(String.Format("The {0} filter is not supported.", filterName));