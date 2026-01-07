


using Plugin.Maui.Audio;

namespace TrafficEscape2
{
    public partial class MainPage : ContentPage
    {
        private Player player;
        private List<Enemy> enemies = new();
        private List<Bullet> bullets = new();

        private IDispatcherTimer gameTimer;
        private IDispatcherTimer enemySpawnTimer;
        private IDispatcherTimer powerUpSpawnTimer;
        private PowerUp activePowerUp;

        private int score = 0;
        private double enemySpeedMultiplier = 1.0;
        private double difficultyMultiplier = 1.0;

        public string difficultyModifier { get; set; } = "normal";
        private int lives = 3;
        private bool isGameRunning = false;
        private bool isCountdownRunning = false;
        
        private double canvasWidth;
        private double canvasHeight;
        private double lastPanX = 0;
        private double lastPanY = 0;

        private IAudioPlayer countdownPlayer;
        private IAudioPlayer bgmPlayer;
        private IAudioPlayer crashPlayer;
        private IAudioPlayer gameOverPlayer;

        private List<PowerUp> powerUps = new();
        private bool bulletsPowerActive = false;
        private double autoShootInterval = 200; // milliseconds
        private DateTime lastAutoShootTime = DateTime.Now;

        private static readonly Random rand = new Random();

        public int Score
        {
            get { return score; }
            set
            {
                score = value;
                OnPropertyChanged();
            }
        }

        public MainPage()
        {
            InitializeComponent();
            InitialiseTimersandGestures();
            BindingContext = this;

            // Preload audio (CRITICAL)
            countdownPlayer = AudioManager.Current.CreatePlayer(
                FileSystem.OpenAppPackageFileAsync("rsg.mp3").Result);

            bgmPlayer = AudioManager.Current.CreatePlayer(
                FileSystem.OpenAppPackageFileAsync("carsounds.mp3").Result);
            bgmPlayer.Loop = true;

            crashPlayer = AudioManager.Current.CreatePlayer(
                FileSystem.OpenAppPackageFileAsync("carcrash.mp3").Result);

            gameOverPlayer = AudioManager.Current.CreatePlayer(
                FileSystem.OpenAppPackageFileAsync("gameover.mp3").Result);

            difficultyModifier = Preferences.Get("Difficulty", "normal");
            ApplyDifficulty();
        }
        private void TrySpawnPowerUp()
        {
            // Only spawn if no active power-up exists
            if (activePowerUp != null) return;

            // Reduce frequency (0.2% chance per frame)
            if (rand.NextDouble() < 0.002)
            {
                double padding = 70; // avoid edges
                double x = rand.NextDouble() * (canvasWidth - 2 * padding) + padding;
                double y = rand.NextDouble() * (canvasHeight - 2 * padding) + padding;

                activePowerUp = new PowerUp(x, y);
                powerUps.Add(activePowerUp);
                GameCanvas.Children.Add(activePowerUp.Visual);
                AbsoluteLayout.SetLayoutBounds(activePowerUp.Visual,
                    new Rect(activePowerUp.X - 20, activePowerUp.Y - 20, 40, 40));

                AutoRemovePowerUp(activePowerUp);
            }
        }
        public void InitialiseTimersandGestures()
        {
            // Add pan gesture for continuous movement
            var panGesture = new PanGestureRecognizer();
            panGesture.PanUpdated += OnPanUpdated;
            GameCanvas.GestureRecognizers.Add(panGesture);

            powerUpSpawnTimer = Dispatcher.CreateTimer();
            powerUpSpawnTimer.Interval = TimeSpan.FromSeconds(12); // spawn every 12 sec
            powerUpSpawnTimer.Tick += (s, e) => TrySpawnPowerUp();
            powerUpSpawnTimer.Start();

            // Setup game loop timer using DispatcherTimer (60 FPS)
            gameTimer = Dispatcher.CreateTimer();
            gameTimer.Interval = TimeSpan.FromMilliseconds(16);
            gameTimer.Tick += OnGameTick;

            enemySpawnTimer = Dispatcher.CreateTimer();
            enemySpawnTimer.Interval = TimeSpan.FromSeconds(1);
            enemySpawnTimer.Tick += OnEnemySpawn;

        }
        protected override void OnSizeAllocated(double width, double height)
        {
            base.OnSizeAllocated(width, height);
            if (width > 0 && height > 0)
            {
                canvasWidth = width;
                canvasHeight = height - 65; // Account for header
            }
        }
        private async void OnStartClicked(object sender, EventArgs e)
        {
            await StartCountdown();
        }

        private async void StartGame()
        {
            if (isGameRunning) return;
            ApplyDifficulty();
            isGameRunning = true;
            score = 0;
            lives = 3;
            enemies.Clear();
            GameCanvas.Children.Clear();

            GameOverOverlay.IsVisible = false;
            StartButton.IsEnabled = false;

            // START GAME NOW
            gameTimer.Start();
            enemySpawnTimer.Start();
            UpdateUI();

            player = new Player(canvasWidth / 2, canvasHeight - 50);
            GameCanvas.Children.Add(player.Visual);
            AbsoluteLayout.SetLayoutBounds(player.Visual,
                new Rect(player.X - player.Size / 2, player.Y - player.Size / 2, player.Size, player.Size));
        }
        
        private async void OnGameTick(object sender, EventArgs e)
        {
            if (!isGameRunning) return;

            // Increase difficulty every 1000 score
            double scoreMultiplier = 1.0 + (Score / 1000) * 2.0;
            enemySpeedMultiplier = difficultyMultiplier * scoreMultiplier;

            TrySpawnPowerUp();

            // Update all bullets
            for (int i = bullets.Count - 1; i >= 0; i--)
             {
               bullets[i].Update();

            // Update enemy position
              AbsoluteLayout.SetLayoutBounds(bullets[i].Visual,
                 new Rect(bullets[i].X - 3,
                       bullets[i].Y - 10,
                        6, 20));
              if (!bullets[i].IsOnScreen(canvasWidth, canvasHeight))
             {
                  GameCanvas.Children.Remove(bullets[i].Visual);
                  bullets.RemoveAt(i);

              }
            }

            for (int i = powerUps.Count - 1; i >= 0; i--)
            {
                if (!powerUps[i].IsActive) continue;

                double halfP = player.Size / 2;
                double halfPU = 20; // half size of powerup

                double pLeft = player.X - halfP;
                double pRight = player.X + halfP;
                double pTop = player.Y - halfP;
                double pBottom = player.Y + halfP;

                double puLeft = powerUps[i].X - halfPU;
                double puRight = powerUps[i].X + halfPU;
                double puTop = powerUps[i].Y - halfPU;
                double puBottom = powerUps[i].Y + halfPU;

                bool hit = !(pRight < puLeft || pLeft > puRight || pBottom < puTop || pTop > puBottom);

                if (hit)
                {
                    powerUps[i].IsActive = false;
                    GameCanvas.Children.Remove(powerUps[i].Visual);

                    // reset activePowerUp so new ones can spawn
                    activePowerUp = null;

                    ActivateBulletsPowerUp();
                }
            }

            if (bulletsPowerActive)
            {
                if ((DateTime.Now - lastAutoShootTime).TotalMilliseconds >= autoShootInterval)
                {
                    lastAutoShootTime = DateTime.Now;

                    // shoot a bullet straight upwards
                    double dx = 0;
                    double dy = -1;

                    Bullet bullet = new Bullet(player.X, player.Y - player.Size / 2, dx, dy);
                    bullets.Add(bullet);
                    GameCanvas.Children.Add(bullet.Visual);
                    AbsoluteLayout.SetLayoutBounds(bullet.Visual, new Rect(bullet.X - 3, bullet.Y - 10, 6, 20));
                }
            }

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


            // Update all enemies
            for (int i = enemies.Count - 1; i >= 0; i--)
            {
                enemies[i].Update(canvasWidth, canvasHeight);

                // Remove enemies that went off screen
                if (enemies[i].isOffScreen)
                {
                    GameCanvas.Children.Remove(enemies[i].Visual);
                    enemies.RemoveAt(i);
                    Score += 100;   // Optional: reward score
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
                    Shake();       

                    GameCanvas.Children.Remove(enemies[i].Visual);
                    enemies.RemoveAt(i);

                    bgmPlayer.Pause();

                    crashPlayer.Stop();
                    crashPlayer.Seek(0);
                    crashPlayer.Play();

                    bgmPlayer.Play();

                    LoseLife();
                    continue;
                }

            }
        }

        private async void AutoRemovePowerUp(PowerUp pu)
        {
            await Task.Delay(10000); // 10 seconds
            if (pu.IsActive)
            {
                pu.IsActive = false;
                GameCanvas.Children.Remove(pu.Visual);
                if (activePowerUp == pu)
                    activePowerUp = null;
            }
        }


        private bool CheckBulletCollision(Bullet b, Enemy e)
        {
            double halfE = e.Size / 2;
            return !(b.X < e.X - halfE || b.X > e.X + halfE || b.Y < e.Y - halfE || b.Y > e.Y + halfE);
        }

        private async void ActivateBulletsPowerUp()
        {
            bulletsPowerActive = true;

            // Power-up lasts 5 seconds
            await Task.Delay(5000);

            bulletsPowerActive = false;
        }

        private void OnEnemySpawn(object sender, EventArgs e)
        {
            if (!isGameRunning) return;
            SpawnEnemy();
        }

        private void SpawnEnemy()
        {
           // Random rand = new Random();
            double y = 0;


            // Middle 70% of the screen is "road"
            double roadMinX = canvasWidth * 0.1;
            double roadMaxX = canvasWidth * 0.9;

            double enemyHalf = 20; // half enemy size (adjust if needed)

            double x = rand.NextDouble() *
                      (roadMaxX - roadMinX - enemyHalf * 2)
                      + roadMinX + enemyHalf;


            Enemy enemy;
            enemy = new Enemy(x, y);
            enemy.speed *= enemySpeedMultiplier;
            enemies.Add(enemy);
            GameCanvas.Children.Add(enemy.Visual);
            AbsoluteLayout.SetLayoutBounds(enemy.Visual,
                new Rect(enemy.X - enemy.Size / 2, enemy.Y - enemy.Size / 2, enemy.Size, enemy.Size));

            
        }

        private void OnPanUpdated(object sender, PanUpdatedEventArgs e)
        {
            if (!isGameRunning) return;

            switch (e.StatusType)
            {
                case GestureStatus.Started:
                    // Reset tracking at start of gesture
                    lastPanX = e.TotalX;
                    lastPanY = e.TotalY;
                    break;

                case GestureStatus.Running:
                    {
                        // Only move by the *change* in pan, not the total
                        // Divide by 2 to reduce sensitivity
                        double deltaX = (e.TotalX - lastPanX);
                        double deltaY = (e.TotalY - lastPanY);

                        lastPanX = e.TotalX;
                        lastPanY = e.TotalY;

                        double newX = player.X + deltaX;
                        double newY = player.Y + deltaY;
                        double roadMinX = canvasWidth * 0.1 + player.Size / 2;
                        double roadMaxX = canvasWidth * 0.9 - player.Size / 2;

                        newX = Math.Clamp(newX, roadMinX, roadMaxX);
                        
                        newY = Math.Clamp(newY, player.Size / 2, canvasHeight - player.Size / 2);
                        MovePlayer(newX, newY);
                        break;
                    }

                case GestureStatus.Completed:
                    break;
            }
        }

        private void MovePlayer(double targetX, double targetY)
        {
            player.MoveTo(targetX, targetY);
            AbsoluteLayout.SetLayoutBounds(player.Visual,
                new Rect(player.X - player.Size / 2, player.Y - player.Size / 2, player.Size, player.Size));
        }

        private bool CheckCollision(Player p, Enemy e)
        {
            double halfP = p.Size / 2;
            double halfE = e.Size / 2;

            double pLeft = p.X - halfP;
            double pRight = p.X + halfP;
            double pTop = p.Y - halfP;
            double pBottom = p.Y + halfP;

            double eLeft = e.X - halfE;
            double eRight = e.X + halfE;
            double eTop = e.Y - halfE;
            double eBottom = e.Y + halfE;

            return !(pRight < eLeft ||
                     pLeft > eRight ||
                     pBottom < eTop ||
                     pTop > eBottom);
        }

        private void LoseLife()
        {
            lives--;
            UpdateUI();

            if (lives <= 0)
            {
                EndGame();
            }
        }

        private void UpdateUI()
        {
            LivesLabel.Text = $"Lives: {lives}";
        }

        private async void EndGame()
        {
           
            isGameRunning = false;
            gameTimer?.Stop();
            enemySpawnTimer?.Stop();
            
           

            bgmPlayer.Stop();

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

        private async void SettingsButton_ClickedAsync(object sender, EventArgs e)
        {
            if (!isGameRunning)
            {
                var settingsPage = new Settings();
                settingsPage.DifficultySelected += (difficulty) =>
                {
                    {
                        difficultyModifier = difficulty;

                        if (difficulty == "hard")
                            difficultyMultiplier = 5;
                        else if (difficulty == "easy")
                            difficultyMultiplier = 0.5;
                        else
                            difficultyMultiplier = 1;
                    }
                    ;
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

        private async Task StartCountdown()
        {
            GameCanvas.Children.Clear();
            if (!GameCanvas.Children.Contains(CountdownLabel))
            {
                GameCanvas.Children.Add(CountdownLabel);
            }

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

        private void SaveHighScore()
        {
            int currentHighScore = Preferences.Get("HighScore", 0);

            if (Score > currentHighScore)
            {
                Preferences.Set("HighScore", Score);
            }
        }

    }
    } 

