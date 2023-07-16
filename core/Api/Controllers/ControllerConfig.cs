namespace DividendsHelper.Core.Controllers; 
public interface IControllerConfig {
    public bool EnableCreateAction { get; }
    public bool EnableReadAction { get; }
    public bool EnableUpdateAction { get; }
    public bool EnableDeleteAction { get; }

}

public abstract class ControllerConfig : IControllerConfig {
    public virtual bool EnableCreateAction => false;
    public virtual bool EnableReadAction => true;
    public virtual bool EnableUpdateAction => false;
    public virtual bool EnableDeleteAction => false;
}