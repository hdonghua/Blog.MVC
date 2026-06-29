namespace Blog.MVC.IServices.Storage;

public class StorageNotFoundException : StorageException
{
    public StorageNotFoundException(string message) : base(message)
    {
    }
}
