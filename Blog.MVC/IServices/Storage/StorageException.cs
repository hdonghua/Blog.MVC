namespace Blog.MVC.IServices.Storage;

public class StorageException : Exception
{
    public StorageException(string message) : base(message)
    {
    }
}
