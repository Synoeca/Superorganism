using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Superorganism.Core.Managers;
using Superorganism.ScreenManagement;
using Superorganism.Screens;
using MonoGame.Extended.Tiled;
using Color = Microsoft.Xna.Framework.Color;
using Superorganism.Content.PipelineReaders; // Ensure this namespace is included

namespace Superorganism;

public class Superorganism : Game
{
    private readonly ScreenManager _screenManager;
    public DisplayMode DisplayMode;
    public GraphicsDeviceManager Graphics;
    public GameAudioManager GameAudioManager;

    public Superorganism()
    {
        Graphics = new GraphicsDeviceManager(this);
        Graphics.GraphicsProfile = GraphicsProfile.HiDef;
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
        Window.IsBorderless = true;

        ScreenFactory screenFactory = new();
        Services.AddService(typeof(IScreenFactory), screenFactory);

        DisplayMode = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode;

        _screenManager = new ScreenManager(this);
        _screenManager.GraphicsDeviceManager = Graphics;
        _screenManager.DisplayMode = DisplayMode;
        _screenManager.SetDefaultGraphicsSettings();

        GameAudioManager = new GameAudioManager(new ContentManager(_screenManager.Game.Services, "Content"));
        _screenManager.GameAudioManager = GameAudioManager;
        Components.Add(_screenManager);

        AddInitialScreens();
        _screenManager.GameAudioManager.Initialize(OptionsMenuScreen.SoundEffectVolume,
            OptionsMenuScreen.BackgroundMusicVolume);
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
        // Register custom content readers
        ContentReaders.Register(Content);
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