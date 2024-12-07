using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Superorganism.ScreenManagement;
using Superorganism.Screens;
using Color = Microsoft.Xna.Framework.Color;

namespace Superorganism;

public class Superorganism : Game
{
	private readonly ScreenManager _screenManager;
    public DisplayMode DisplayMode;
	public GraphicsDeviceManager Graphics;

	public Superorganism()
	{
		Graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
		IsMouseVisible = true;

        Window.IsBorderless = true;

		ScreenFactory screenFactory = new();
		Services.AddService(typeof(IScreenFactory), screenFactory);

        DisplayMode = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode;

        _screenManager = new ScreenManager(this);
		_screenManager.GraphicsDeviceManager = Graphics;
        _screenManager.DisplayMode = DisplayMode;
		Components.Add(_screenManager);

        AddInitialScreens();
	}

	private void AddInitialScreens()
	{
		_screenManager.AddScreen(new BackgroundScreen(), null);
		_screenManager.AddScreen(new MainMenuScreen(), null);
		_screenManager.AddScreen(new SplashScreen(), null);
	}

	protected override void Initialize()
	{
		base.Initialize();
	}

	protected override void LoadContent()
	{
	}

	protected override void Update(GameTime gameTime)
	{
		base.Update(gameTime);
	}

	protected override void Draw(GameTime gameTime)
	{
		GraphicsDevice.Clear(Color.CornflowerBlue);
		base.Draw(gameTime); // The real drawing happens inside the ScreenManager component
	}
}