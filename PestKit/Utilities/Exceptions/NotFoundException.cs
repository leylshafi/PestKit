namespace PestKit.Utilities.Exceptions
{
    public class NotFoundException:Exception
    {
        public NotFoundException(string error):base(error) { }
    }
}
