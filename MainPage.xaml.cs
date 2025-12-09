
namespace TrafficEscape2
{
    public partial class MainPage : ContentPage
    {
        private Player player;
        private List<Enemy> enemies = new();
        //private List<Bullet> bullets = new();
        private IDispatcherTimer gameTimer;
        private IDispatcherTimer enemySpawnTimer;

        private int score = 0;
        private int lives = 3;
        private bool isGameRunning = false;

        //private const int MaxBullets = 5;
        private double canvasWidth;
        private double canvasHeight;
        private double lastPanX = 0;
        private double lastPanY = 0;

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
        }

        public void InitialiseTimersandGestures()
        {
            // Add pan gesture for continuous movement
            var panGesture = new PanGestureRecognizer();
            panGesture.PanUpdated += OnPanUpdated;
            GameCanvas.GestureRecognizers.Add(panGesture);

            // Keep tap gesture for shooting
            // var tapGesture = new TapGestureRecognizer();
            // tapGesture.Tapped += OnCanvasTapped;
            //  GameCanvas.GestureRecognizers.Add(tapGesture);

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

        private void OnStartClicked(object sender, EventArgs e)
        {
            StartGame();
        }

        private void StartGame()
        {
            if (isGameRunning) return;

            isGameRunning = true;
            score = 0;
            lives = 3;
            enemies.Clear();
            // bullets.Clear();
            GameCanvas.Children.Clear();
            GameOverOverlay.IsVisible = false;
            StartButton.IsEnabled = false;
            gameTimer.Start();
            enemySpawnTimer.Start();

            UpdateUI();

            // Create player in center
            player = new Player(canvasWidth/2, canvasHeight - 50);
            GameCanvas.Children.Add(player.Visual);
            AbsoluteLayout.SetLayoutBounds(player.Visual,
                new Rect(player.X - player.Size / 2, player.Y - player.Size / 2, player.Size, player.Size));


        }

        private void OnGameTick(object sender, EventArgs e)
        {
            if (!isGameRunning) return;

            // Update all bullets
            //for (int i = bullets.Count - 1; i >= 0; i--)
            // {
            //   bullets[i].Update();

            // Update enemy position
            //  AbsoluteLayout.SetLayoutBounds(bullets[i].Visual,
            //     new Rect(bullets[i].X - 3,
            //           bullets[i].Y - 10,
            //            6, 20));
            //  if (!bullets[i].IsOnScreen(canvasWidth, canvasHeight))
            // {
            //      GameCanvas.Children.Remove(bullets[i].Visual);
            //      bullets.RemoveAt(i);

            //  }
            // }

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
                    GameCanvas.Children.Remove(enemies[i].Visual);
                    enemies.RemoveAt(i);
                    LoseLife();
                    continue;
                }

                ScoreLabel.Text = Score.ToString();



            }
        }

        private void OnEnemySpawn(object sender, EventArgs e)
        {
            if (!isGameRunning) return;
            SpawnEnemy();
        }

        private void SpawnEnemy()
        {
           // Random rand = new Random();
            double x, y;

            // Spawn at random edge of screen
            double width = canvasWidth - 30;
            
                    x = rand.NextDouble() * width;
                    y = 0;
               

            Enemy enemy;
            enemy = new Enemy(x, y);
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

                        newX = Math.Clamp(newX, player.Size /2, canvasWidth - player.Size / 2);
                        newY = Math.Clamp(newY, player.Size / 2, canvasHeight - player.Size / 2);
                        MovePlayer(newX, newY);
                        break;
                    }

                case GestureStatus.Completed:
                    break;
            }
        }

        //private void OnCanvasTapped(object sender, TappedEventArgs e)
        // {
        // if (!isGameRunning) return;

        // Tap to shoot in direction of tap
        //   Point pt = (Point)e.GetPosition(GameCanvas);
        //   ShootTowards(pt.X, pt.Y);

        //  }

        private void MovePlayer(double targetX, double targetY)
        {
            player.MoveTo(targetX, targetY);
            AbsoluteLayout.SetLayoutBounds(player.Visual,
                new Rect(player.X - player.Size / 2, player.Y - player.Size / 2, player.Size, player.Size));
        }

        /* private void ShootTowards(double targetX, double targetY)
         {
             if (bullets.Count >= MaxBullets) return;

             // Calculate direction to tap point
             double dx = targetX - player.X;
             double dy = targetY - player.Y;

             double length = Math.Sqrt(dx * dx + dy * dy);
             dx /= length;
             dy /= length;

             Bullet bullet = new Bullet(player.X, player.Y, dx, dy);
             bullets.Add(bullet);
             GameCanvas.Children.Add(bullet.Visual);
             AbsoluteLayout.SetLayoutBounds(bullet.Visual, new Rect(bullet.X - 3, bullet.Y - 10, 6, 20));
             new Rect(bullet.X - 3, bullet.Y - 10, 6, 20);
             double angle = Math.Atan2(dy, dx) * 180 / Math.PI;
             player.RotatePlayer(angle + 90);


         }*/

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

        private void EndGame()
        {
            isGameRunning = false;
            gameTimer?.Stop();
            enemySpawnTimer?.Stop();

            GameOverOverlay.IsVisible = true;
        }

        private void OnPlayAgainClicked(object sender, EventArgs e)
        {
            StartGame();
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            gameTimer?.Stop();
            enemySpawnTimer?.Stop();
        }




        private async void SettingsButton_ClickedAsync(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new Settings());
        }
    } }

