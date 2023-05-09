public struct MsgObjectStarted<T>
{
    public T obj;

    public MsgObjectStarted(T obj)
    {
        this.obj = obj;
    }
}
