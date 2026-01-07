using Microsoft.Maui.Devices;
using Plugin.Maui.Audio;


namespace TrafficEscape2
{
    public partial class MainPage : ContentPage
    {
        // ------------------ GAME ENTITIES ------------------
        private Player player;                       // The player's car
        private List<Enemy> enemies = new();         // List of enemies currently in the game
        private List<Bullet> bullets = new();       // List of bullets currently on screen

        private IDispatcherTimer gameTimer;          // Main game loop timer (60 FPS)
        private IDispatcherTimer enemySpawnTimer;    // Timer for spawning enemies
        private IDispatcherTimer powerUpSpawnTimer;  // Timer for spawning power-ups
        private PowerUp activePowerUp;               // Currently active power-up

        // ------------------ GAME STATE ------------------
        private int score = 0;                        // Player's score
        private double enemySpeedMultiplier = 1.0;   // Multiplier for enemy speed
        private double difficultyMultiplier = 1.0;   // Multiplier based on difficulty

        public string difficultyModifier { get; set; } = "normal"; // Difficulty setting
        private int lives = 3;                        // Player lives
        private bool isGameRunning = false;           // True if game is running
        private bool isCountdownRunning = false;      // True if countdown is active

        // ------------------ CANVAS & MOVEMENT ------------------
        private double canvasWidth;                   // Width of game canvas
        private double canvasHeight;                  // Height of game canvas
        private double lastPanX = 0;                  // Last pan X position for movement
        private double lastPanY = 0;                  // Last pan Y position for movement

        // ------------------ AUDIO ------------------
        private IAudioPlayer countdownPlayer;         // Countdown sound
        private IAudioPlayer bgmPlayer;               // Background music
        private IAudioPlayer crashPlayer;             // Crash sound
        private IAudioPlayer gameOverPlayer;          // Game over sound

        // ------------------ POWER-UPS ------------------
        private List<PowerUp> powerUps = new();      // All power-ups in the game
        private bool bulletsPowerActive = false;     // Whether auto-shoot power-up is active
        private double autoShootInterval = 200;      // Interval between auto-shoot bullets (ms)
        private DateTime lastAutoShootTime = DateTime.Now; // Track last auto-shoot time

        private static readonly Random rand = new Random(); // Random generator

        // ------------------ SCORE PROPERTY ------------------
        public int Score
        {
            get { return score; }
            set
            {
                score = value;
                OnPropertyChanged(); // Notify UI of score change
            }
        }

        // ------------------ CONSTRUCTOR ------------------
        public MainPage()
        {
            InitializeComponent();                   // Initialize UI components
            AdjustButtonSizesForAndroid();
            InitialiseTimersandGestures();           // Setup timers and pan gestures
            BindingContext = this;                   // Set binding context for data binding

            // ------------------ AUDIO SETUP ------------------
            countdownPlayer = AudioManager.Current.CreatePlayer(
                FileSystem.OpenAppPackageFileAsync("rsg.mp3").Result);

            bgmPlayer = AudioManager.Current.CreatePlayer(
                FileSystem.OpenAppPackageFileAsync("carsounds.mp3").Result);
            bgmPlayer.Loop = true;                   // Loop background music

            crashPlayer = AudioManager.Current.CreatePlayer(
                FileSystem.OpenAppPackageFileAsync("carcrash.mp3").Result);

            gameOverPlayer = AudioManager.Current.CreatePlayer(
                FileSystem.OpenAppPackageFileAsync("gameover.mp3").Result);

            // Load difficulty from preferences
            difficultyModifier = Preferences.Get("Difficulty", "normal");
            ApplyDifficulty();
        }

        // ------------------ POWER-UP SPAWNING ------------------
        private void TrySpawnPowerUp()
        {
            // Only spawn if no active power-up exists
            if (activePowerUp != null) return;

            // Very low chance per frame (0.2%) to spawn a power-up
            if (rand.NextDouble() < 0.002)
            {
                double padding = 70; // Distance from edges
                double x = rand.NextDouble() * (canvasWidth - 2 * padding) + padding;
                double y = rand.NextDouble() * (canvasHeight - 2 * padding) + padding;

                activePowerUp = new PowerUp(x, y);
                powerUps.Add(activePowerUp);
                GameCanvas.Children.Add(activePowerUp.Visual);
                AbsoluteLayout.SetLayoutBounds(activePowerUp.Visual,
                    new Rect(activePowerUp.X - 20, activePowerUp.Y - 20, 40, 40));

                // Automatically remove power-up after 10 seconds
                AutoRemovePowerUp(activePowerUp);
            }
        }

        // ------------------ TIMER AND GESTURE INITIALIZATION ------------------
        public void InitialiseTimersandGestures()
        {
            // Add pan gesture for player movement
            var panGesture = new PanGestureRecognizer();
            panGesture.PanUpdated += OnPanUpdated;
            GameCanvas.GestureRecognizers.Add(panGesture);

            // Power-up spawn timer (every 12 seconds)
            powerUpSpawnTimer = Dispatcher.CreateTimer();
            powerUpSpawnTimer.Interval = TimeSpan.FromSeconds(12);
            powerUpSpawnTimer.Tick += (s, e) => TrySpawnPowerUp();
            powerUpSpawnTimer.Start();

            // Main game loop timer (60 FPS)
            gameTimer = Dispatcher.CreateTimer();
            gameTimer.Interval = TimeSpan.FromMilliseconds(16);
            gameTimer.Tick += OnGameTick;

            // Enemy spawn timer (every 1 second)
            enemySpawnTimer = Dispatcher.CreateTimer();
            enemySpawnTimer.Interval = TimeSpan.FromSeconds(1);
            enemySpawnTimer.Tick += OnEnemySpawn;
        }

        // ------------------ HANDLE CANVAS SIZE ------------------
        protected override void OnSizeAllocated(double width, double height)
        {
            base.OnSizeAllocated(width, height);
            if (width > 0 && height > 0)
            {
                canvasWidth = width;
                canvasHeight = height - 65; // Account for header
            }
        }

        // ------------------ START BUTTON ------------------
        private async void OnStartClicked(object sender, EventArgs e)
        {
            await StartCountdown(); // Start countdown before the game
        }

        // ------------------ START GAME ------------------
        private async void StartGame()
        {
            if (isGameRunning) return; // Prevent restarting if already running
            ApplyDifficulty();
            isGameRunning = true;
            score = 0;
            lives = 3;
            enemies.Clear();
            GameCanvas.Children.Clear();

            GameOverOverlay.IsVisible = false;
            StartButton.IsEnabled = false;

            // Start game timers
            gameTimer.Start();
            enemySpawnTimer.Start();
            UpdateUI();

            // Spawn player at bottom center
            player = new Player(canvasWidth / 2, canvasHeight - 50);
            GameCanvas.Children.Add(player.Visual);
            AbsoluteLayout.SetLayoutBounds(player.Visual,
                new Rect(player.X - player.Size / 2, player.Y - player.Size / 2, player.Size, player.Size));
        }

        // ------------------ GAME LOOP ------------------
        private async void OnGameTick(object sender, EventArgs e)
        {
            if (!isGameRunning) return;

            // Increase difficulty over time
            double scoreMultiplier = 1.0 + (Score / 1000) * 2.0;
            enemySpeedMultiplier = difficultyMultiplier * scoreMultiplier;

            // Try spawning a power-up
            TrySpawnPowerUp();

            // ------------------ BULLETS UPDATE ------------------
            for (int i = bullets.Count - 1; i >= 0; i--)
            {
                bullets[i].Update();
                AbsoluteLayout.SetLayoutBounds(bullets[i].Visual,
                    new Rect(bullets[i].X - 3, bullets[i].Y - 10, 6, 20));

                // Remove bullets off-screen
                if (!bullets[i].IsOnScreen(canvasWidth, canvasHeight))
                {
                    GameCanvas.Children.Remove(bullets[i].Visual);
                    bullets.RemoveAt(i);
                }
            }

            // ------------------ POWER-UP COLLECTION ------------------
            for (int i = powerUps.Count - 1; i >= 0; i--)
            {
                if (!powerUps[i].IsActive) continue;

                double halfP = player.Size / 2;    // Player half-size
                double halfPU = 20;                 // Power-up half-size (40x40)

                // Player bounding box
                double pLeft = player.X - halfP;
                double pRight = player.X + halfP;
                double pTop = player.Y - halfP;
                double pBottom = player.Y + halfP;

                // Power-up bounding box
                double puLeft = powerUps[i].X - halfPU;
                double puRight = powerUps[i].X + halfPU;
                double puTop = powerUps[i].Y - halfPU;
                double puBottom = powerUps[i].Y + halfPU;

                // Collision logic (same as above)
                bool hit = !(pRight < puLeft || pLeft > puRight || pBottom < puTop || pTop > puBottom);

                if (hit)
                {
                    powerUps[i].IsActive = false;
                    GameCanvas.Children.Remove(powerUps[i].Visual);
                    activePowerUp = null;  // Allow new power-ups to spawn
                    ActivateBulletsPowerUp(); // Activate auto-shoot effect
                }
            }

            // ------------------ AUTO-SHOOT POWER-UP ------------------
            if (bulletsPowerActive)
            {
                if ((DateTime.Now - lastAutoShootTime).TotalMilliseconds >= autoShootInterval)
                {
                    lastAutoShootTime = DateTime.Now;

                    // Shoot a bullet straight upwards
                    Bullet bullet = new Bullet(player.X, player.Y - player.Size / 2, 0, -1);
                    bullets.Add(bullet);
                    GameCanvas.Children.Add(bullet.Visual);
                    AbsoluteLayout.SetLayoutBounds(bullet.Visual, new Rect(bullet.X - 3, bullet.Y - 10, 6, 20));
                }
            }

            // ------------------ BULLET VS ENEMY COLLISION ------------------
            for (int i = bullets.Count - 1; i >= 0; i--)
            {
                bullets[i].Update();

                for (int j = enemies.Count - 1; j >= 0; j--)
                {
                    if (CheckBulletCollision(bullets[i], enemies[j]))
                    {
                        GameCanvas.Children.Remove(enemies[j].Visual);
                        enemies.RemoveAt(j);

                        GameCanvas.Children.Remove(bullets[i].Visual);
                        bullets.RemoveAt(i);
                        Score += 100;
                        break;
                    }
                }
            }

            // ------------------ ENEMY UPDATE ------------------
            for (int i = enemies.Count - 1; i >= 0; i--)
            {
                enemies[i].Update(canvasWidth, canvasHeight);

                // Remove off-screen enemies
                if (enemies[i].isOffScreen)
                {
                    GameCanvas.Children.Remove(enemies[i].Visual);
                    enemies.RemoveAt(i);
                    Score += 100;
                    ScoreLabel.Text = score.ToString();
                    continue;
                }

                // Update enemy position
                AbsoluteLayout.SetLayoutBounds(enemies[i].Visual,
                    new Rect(enemies[i].X - enemies[i].Size / 2,
                             enemies[i].Y - enemies[i].Size / 2,
                             enemies[i].Size, enemies[i].Size));

                // Check collision with player
                if (CheckCollision(player, enemies[i]))
                {
                    Shake(); // Shake screen on collision

                    GameCanvas.Children.Remove(enemies[i].Visual);
                    enemies.RemoveAt(i);

                    // Play crash sound
                    bgmPlayer.Pause();
                    crashPlayer.Stop();
                    crashPlayer.Seek(0);
                    crashPlayer.Play();
                    bgmPlayer.Play();

                    LoseLife(); // Decrease life
                    continue;
                }
            }
        }

        // ------------------ POWER-UP AUTO REMOVE ------------------
        private async void AutoRemovePowerUp(PowerUp pu)
        {
            await Task.Delay(10000); // Remove after 10 seconds
            if (pu.IsActive)
            {
                pu.IsActive = false;
                GameCanvas.Children.Remove(pu.Visual);
                if (activePowerUp == pu)
                    activePowerUp = null;
            }
        }

        // ------------------ BULLET VS ENEMY COLLISION ------------------
        private bool CheckBulletCollision(Bullet b, Enemy e)
        {
            // Calculate enemy half-size for easier collision
            double halfE = e.Size / 2;

            // Check if bullet is outside enemy's bounding box
            // If bullet is outside any side, return false
            // Otherwise, bullet is inside enemy → collision occurs
            return !(b.X < e.X - halfE ||  // Bullet left of enemy
                     b.X > e.X + halfE ||  // Bullet right of enemy
                     b.Y < e.Y - halfE ||  // Bullet above enemy
                     b.Y > e.Y + halfE);   // Bullet below enemy
        }

        // ------------------ ACTIVATE BULLETS POWER-UP ------------------
        private async void ActivateBulletsPowerUp()
        {
            bulletsPowerActive = true;

            // Power-up lasts 5 seconds
            await Task.Delay(5000);

            bulletsPowerActive = false;
        }

        // ------------------ ENEMY SPAWN ------------------
        private void OnEnemySpawn(object sender, EventArgs e)
        {
            if (!isGameRunning) return;
            SpawnEnemy();
        }

        private void SpawnEnemy()
        {
            double y = 0;

            // Middle 70% of screen is road
            double roadMinX = canvasWidth * 0.1;
            double roadMaxX = canvasWidth * 0.9;

            double enemyHalf = 20; // half enemy size
            double x = rand.NextDouble() * (roadMaxX - roadMinX - enemyHalf * 2) + roadMinX + enemyHalf;

            Enemy enemy = new Enemy(x, y);
            enemy.speed *= enemySpeedMultiplier;
            enemies.Add(enemy);
            GameCanvas.Children.Add(enemy.Visual);
            AbsoluteLayout.SetLayoutBounds(enemy.Visual,
                new Rect(enemy.X - enemy.Size / 2, enemy.Y - enemy.Size / 2, enemy.Size, enemy.Size));
        }

        // ------------------ PLAYER MOVEMENT ------------------
        private void OnPanUpdated(object sender, PanUpdatedEventArgs e)
        {
            if (!isGameRunning) return;

            switch (e.StatusType)
            {
                case GestureStatus.Started:
                    // Reset tracking positions at start of gesture
                    lastPanX = e.TotalX;
                    lastPanY = e.TotalY;
                    break;

                case GestureStatus.Running:
                    // Calculate change in pan since last event
                    double deltaX = e.TotalX - lastPanX;
                    double deltaY = e.TotalY - lastPanY;

                    // Update last positions
                    lastPanX = e.TotalX;
                    lastPanY = e.TotalY;

                    // New target position for player
                    double newX = player.X + deltaX;
                    double newY = player.Y + deltaY;

                    // Keep player inside the road horizontally
                    double roadMinX = canvasWidth * 0.1 + player.Size / 2;
                    double roadMaxX = canvasWidth * 0.9 - player.Size / 2;
                    newX = Math.Clamp(newX, roadMinX, roadMaxX);

                    // Keep player inside canvas vertically
                    newY = Math.Clamp(newY, player.Size / 2, canvasHeight - player.Size / 2);

                    MovePlayer(newX, newY); // Move player to new location
                    break;

                case GestureStatus.Completed:
                    break; // No action needed when pan ends
            }
        }

        private void MovePlayer(double targetX, double targetY)
        {
            player.MoveTo(targetX, targetY);
            AbsoluteLayout.SetLayoutBounds(player.Visual,
                new Rect(player.X - player.Size / 2, player.Y - player.Size / 2, player.Size, player.Size));
        }

        // ------------------ PLAYER VS ENEMY COLLISION ------------------
        private bool CheckCollision(Player p, Enemy e)
        {
            // Calculate half the width/height for easier bounding box calculations
            double halfP = p.Size / 2;
            double halfE = e.Size / 2;

            // Get player bounding box
            double pLeft = p.X - halfP;
            double pRight = p.X + halfP;
            double pTop = p.Y - halfP;
            double pBottom = p.Y + halfP;

            // Get enemy bounding box
            double eLeft = e.X - halfE;
            double eRight = e.X + halfE;
            double eTop = e.Y - halfE;
            double eBottom = e.Y + halfE;

            // Check if boxes overlap:
            // Collision occurs when boxes intersect, so we return true if they do
            // Logic: no collision if one rectangle is completely outside the other
            return !(pRight < eLeft ||   // Player is completely to the left of enemy
                     pLeft > eRight ||   // Player is completely to the right of enemy
                     pBottom < eTop ||   // Player is completely above enemy
                     pTop > eBottom);    // Player is completely below enemy
        }

        // ------------------ LIVES ------------------
        private void LoseLife()
        {
            lives--;
            UpdateUI();

            if (lives <= 0)
                EndGame();
        }

        private void UpdateUI()
        {
            LivesLabel.Text = $"Lives: {lives}";
        }

        // ------------------ GAME OVER ------------------
        private async void EndGame()
        {
            isGameRunning = false;
            gameTimer?.Stop();
            enemySpawnTimer?.Stop();

            bgmPlayer.Stop();

            // Play game over sound
            gameOverPlayer.Stop();
            gameOverPlayer.Seek(0);
            gameOverPlayer.Play();

            GameOverOverlay.IsVisible = true;
            SaveHighScore();
        }

        private async void OnPlayAgainClicked(object sender, EventArgs e)
        {
            GameOverOverlay.IsVisible = false;
            await StartCountdown();
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            gameTimer?.Stop();
            enemySpawnTimer?.Stop();
        }

        // ------------------ SETTINGS ------------------
        private async void SettingsButton_ClickedAsync(object sender, EventArgs e)
        {
            if (!isGameRunning)
            {
                var settingsPage = new Settings();
                settingsPage.DifficultySelected += (difficulty) =>
                {
                    difficultyModifier = difficulty;

                    if (difficulty == "hard")
                        difficultyMultiplier = 5;
                    else if (difficulty == "easy")
                        difficultyMultiplier = 0.5;
                    else
                        difficultyMultiplier = 1;
                };
                await Navigation.PushAsync(settingsPage);
            }
            else
                await FlashSettingsButtonRed();
            return; // Do NOT open settings
        }

        private async Task FlashSettingsButtonRed()
        {
            Color originalColor = SettingsButton.BackgroundColor;

            for (int i = 0; i < 2; i++)
            {
                SettingsButton.BackgroundColor = Colors.Red;
                await Task.Delay(150);
                SettingsButton.BackgroundColor = originalColor;
                await Task.Delay(150);
            }
        }

        private void ApplyDifficulty()
        {
            if (difficultyModifier == "hard")
                difficultyMultiplier = 5;
            else if (difficultyModifier == "easy")
                difficultyMultiplier = 0.5;
            else
                difficultyMultiplier = 1;
        }

        // ------------------ COUNTDOWN BEFORE GAME ------------------
        private async Task StartCountdown()
        {
            GameCanvas.Children.Clear();
            if (!GameCanvas.Children.Contains(CountdownLabel))
                GameCanvas.Children.Add(CountdownLabel);

            if (isCountdownRunning) return;
            isCountdownRunning = true;
            CountdownLabel.IsVisible = true;

            countdownPlayer.Stop();
            countdownPlayer.Seek(0);
            countdownPlayer.Play();

            for (int i = 3; i >= 1; i--)
            {
                CountdownLabel.Text = i.ToString();
                await Task.Delay(1000);
            }

            CountdownLabel.IsVisible = false;
            isCountdownRunning = false;

            StartGame();
            StartBackgroundMusic();
        }

        private void StartBackgroundMusic()
        {
            bgmPlayer.Stop();
            bgmPlayer.Seek(0);
            bgmPlayer.Play();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            bool isDarkMode = Preferences.Get("IsDarkMode", false);
            Application.Current.UserAppTheme =
                isDarkMode ? AppTheme.Dark : AppTheme.Light;
        }

        // ------------------ SCREEN SHAKE EFFECT ------------------
        private async Task Shake()
        {
            for (int i = 0; i < 6; i++)
            {
                GameCanvas.TranslationX = rand.Next(-6, 6);
                GameCanvas.TranslationY = rand.Next(-6, 6);
                await Task.Delay(16);
            }

            GameCanvas.TranslationX = 0;
            GameCanvas.TranslationY = 0;
        }

        // ------------------ HIGH SCORE ------------------
        private void SaveHighScore()
        {
            int currentHighScore = Preferences.Get("HighScore", 0);

            if (Score > currentHighScore)
            {
                Preferences.Set("HighScore", Score);
            }
        }

private void AdjustButtonSizesForAndroid()
    {
        // Only apply on Android
        if (DeviceInfo.Platform == DevicePlatform.Android)
        {
            double scaleFactor = DeviceDisplay.MainDisplayInfo.Density; // typical: 2-3

            // Reduce the size a bit
            StartButton.FontSize = 16 / scaleFactor; // smaller text
            StartButton.WidthRequest = 150 / scaleFactor;
            StartButton.HeightRequest = 50 / scaleFactor;

            SettingsButton.FontSize = 14 / scaleFactor;
            SettingsButton.WidthRequest = 120 / scaleFactor;
            SettingsButton.HeightRequest = 40 / scaleFactor;
        }
    }
}
}
