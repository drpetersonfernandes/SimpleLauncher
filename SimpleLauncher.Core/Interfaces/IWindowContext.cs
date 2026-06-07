namespace SimpleLauncher.Core.Interfaces;

public interface IWindowContext
{
    IntPtr Handle { get; }
    void Show();
    void Hide();
    void Activate();
}
