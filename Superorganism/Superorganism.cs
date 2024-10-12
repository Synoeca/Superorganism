using Microsoft.Xna.Framework;
using Superorganism.Screens;
using Superorganism.StateManagement;

namespace Superorganism;

public class Superorganism : Game
{
	private readonly ScreenManager _screenManager;
	private GraphicsDeviceManager _graphics;

	public Superorganism()
	{
		_graphics = new GraphicsDeviceManager(this);
		Content.RootDirectory = "Content";
		IsMouseVisible = true;

		ScreenFactory screenFactory = new();
		Services.AddService(typeof(IScreenFactory), screenFactory);

		_screenManager = new ScreenManager(this);
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