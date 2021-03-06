﻿namespace MoenenVoxel {

	using UnityEngine;
	using UnityEngine.UI;
	using System.Collections;

    public class DemoStage : MonoBehaviour {

		public static DemoStage Main = null;
		public static int CurrentEnemyNum = 0;
		public static bool Playing = false;


		[Space(4f)]
		public Light MainLight;
		public Transform MainGround;
		public Text KillNum, HighScore, Msg;
		public Image HPBarIMG;
		public Text HPBarTXT;
		public Transform GameOverUI;
		public Text GameOverMSG;
		public Transform MainBar;
		public Text _1;

		[Space(4f)]
		public int DeadEnd = 100;
		public Transform[] Enemys;
		public Transform[] Players;
		public AudioClip[] SFXs;
		[HideInInspector]
		public Transform CurrentPlayer = null;




		private float CurrentSpawnGap = 2f;
		private float LastSpawnTime = -100f;
		private float PrevAlertTime = -100f;
		private Vector2 SpawnRange;
		public int currentKillNum = 0, highScore = 0;
		private int currentPlayerID = 0;
		private Transform PlayerSign;

        private Color LightColor;
		private float AimPitch = 1f;
		private bool FirstStart = true;
		private AudioSource Audio;
		private int combo = 0;

        
        private int level = 1;
        private float timeLeft = 20f;
        private int goal = 1;

        public int maxPowerUpCount = 1;
        public int powerUpCount = 0;
        void Awake () {
			Main = this;
			SpawnRange = new Vector2(MainGround.localScale.x * 0.45f, MainGround.localScale.z * 0.45f);
			LightColor = MainLight.color;
			Audio = GetComponent<AudioSource>();
			GameOverUI.gameObject.SetActive(true);
		}


		void GameStart () {

			Playing = true;
			if (!FirstStart) {
				Audio.time = 0f;
			}
			FirstStart = false;
			GameOverUI.gameObject.SetActive(false);
			CurrentSpawnGap = 2f;
			AimPitch = 1f;
			Audio.pitch = 1f;


			// Player
			CurrentPlayer = Instantiate<GameObject>(Players[currentPlayerID].gameObject).transform;
			CurrentPlayer.position = Vector3.zero;
			CurrentPlayer.rotation = Quaternion.identity;

			// Player Sign Init
			GameObject sign = new GameObject("1P_Sign");
			PlayerSign = sign.transform;
			PlayerSign.rotation = Camera.main.transform.rotation;
			TextMesh tm = sign.AddComponent<TextMesh>();
			tm.text = "[ \"You (  ) hard today.\" \"Yes I have a lot to do\" ]";
			tm.color = new Color(0.8f, 0.8f, 0.8f);
			tm.fontStyle = FontStyle.Bold;
			tm.alignment = TextAlignment.Center;
			tm.anchor = TextAnchor.MiddleCenter;
			tm.characterSize = 0.065f;
			tm.fontSize = 60;
            
            //ScoreInit
            CurrentEnemyNum = 0;
			currentKillNum = 0;
			highScore = PlayerPrefs.GetInt("MoenenVoxel.HighScore", 0);
//			HighScore.text = highScore.ToString("00");
			KillNum.text = "0";
//			FreshBar2();
			SpawnEnemy();
		}

		void Update () {
            if(Playing)
            {
                timeLeft -= Time.deltaTime;
                FreshBar3();
            }
            
            HighScore.text = (Mathf.Ceil(timeLeft)).ToString();

            // Audio Lerp
            Audio.pitch = Mathf.Lerp(Audio.pitch, AimPitch, Time.deltaTime * 0.2f);

			// Light Lerp
			MainLight.color = Color.Lerp(MainLight.color, LightColor, 0.1f);

			// KillTextLerp
			KillNum.transform.localScale = Vector3.Lerp(KillNum.transform.localScale, Vector3.one, 0.4f);
			HighScore.transform.localScale = Vector3.Lerp(HighScore.transform.localScale, Vector3.one, 0.4f);

			// MainBar Lerp
			MainBar.localScale = Vector3.Lerp(MainBar.localScale, Vector3.one, 0.1f);

			// -1 Lerp
			_1.transform.localScale = Vector3.Lerp(_1.transform.localScale, Vector3.one, 0.05f);
			if (_1.transform.localScale.x > 1.01f) {
				_1.enabled = true;
			} else {
				_1.enabled = false;
				combo = 0;
			}


			// Restart
			if ((Input.GetKeyDown(KeyCode.Return) && FirstStart && !Input.GetMouseButton(0) && !Input.GetMouseButton(1)) || (Input.GetKeyDown(KeyCode.Escape) && !Playing)) {
				GameStart();
			}

            if(currentKillNum >= goal)
            {
                timeLeft = 20;
                goal++;
                level++;
                currentKillNum = 0;
//                FreshBar2();
            }

			// Game Over
			if (timeLeft <= 0 || !Playing) {
				if (Playing) {
					Playing = false;
					// UI
					GameOverUI.gameObject.SetActive(true);
					GameOverMSG.text = string.Format(
	@"<size=70>Game Over</size>


You Killed <color=#cc3333ff><size=50>{0}</size></color> Enemys

High Score: <color=#cc3333ff><size=50>{1}</size></color>

Press <size=50><color=#cc3333ff>[ESC]</color></size> to Continue",
						currentKillNum,
						highScore
					);
					GameOverUI.GetComponent<Image>().color = new Color(0.3f, 0.05f, 0.05f, 0.6f);
					// Despawn Player
					Destroy(CurrentPlayer.gameObject);
					Destroy(PlayerSign.gameObject);
					PlaySound(11);
					PlaySound(12, 2f);
					AimPitch = 0f;
				}
				return;
			}


			// Change Player
			if (Input.GetKeyDown(KeyCode.Tab)) {                
                currentPlayerID++;
				currentPlayerID %= Players.Length;
				Vector3 pos = CurrentPlayer.position;
				Quaternion rot = CurrentPlayer.rotation;
				Destroy(CurrentPlayer.gameObject);
				CurrentPlayer = Instantiate<GameObject>(Players[currentPlayerID].gameObject).transform;
				CurrentPlayer.position = pos;
				CurrentPlayer.rotation = rot;
				CurrentPlayer.localScale = Vector3.one * 0.1f;
				TextMesh tm = PlayerSign.GetComponent<TextMesh>();
				tm.text = "[ " + Players[currentPlayerID].gameObject.name + " ]";
				PlaySound(0);
			}

			// Player Sign
			if (CurrentPlayer && PlayerSign) {
				CurrentPlayer.localScale = Vector3.Lerp(CurrentPlayer.localScale, Vector3.one, 0.1f);
				PlayerSign.position = CurrentPlayer.position + Vector3.up * 3f;
			}

			// Player Safe
			if (CurrentPlayer && CurrentPlayer.position.y < MainGround.position.y - 2f) {
				CurrentPlayer.position = Vector3.zero;
			}

//			// Spawn Enemy
//			if (Time.time > LastSpawnTime + CurrentSpawnGap) {
//				LastSpawnTime = Time.time;
//				CurrentSpawnGap *= Mathf.Lerp(0.99f, 0.999f, Mathf.Clamp01((float)CurrentEnemyNum / (float)DeadEnd));
//				CurrentSpawnGap = Mathf.Max(0.3f, CurrentSpawnGap);
//				SpawnEnemy();
//			}

            //Alert
   //         if (CurrentEnemyNum > DeadEnd - 10 && Time.time > PrevAlertTime + Mathf.Lerp(0.1f, 2f, (float)(DeadEnd - CurrentEnemyNum) / 10f)) {
			//	PrevAlertTime = Time.time;
			//	PlaySound(13, 0.6f);
			//	MainBar.transform.localScale = Vector3.one * 1.4f;
			//	MainLight.color = new Color(0.6f, 0.3f, 0.3f);
			//}

		}

		void SpawnEnemy () {
			float id = Random.Range(0f, (float)Enemys.Length - 0.01f);
			GameObject e = Instantiate<GameObject>(Enemys[(int)id].gameObject);
			e.transform.rotation = Quaternion.identity;
			e.transform.localScale = Vector3.one;
			e.transform.position = new Vector3(Random.Range(-SpawnRange.x, SpawnRange.x), 10f, Random.Range(-SpawnRange.y, SpawnRange.y));

			EnemyBehaviour eb = e.GetComponent<EnemyBehaviour>();
			if (eb) {
				eb.MaxHP = eb.HP = (id % 3f) * 20f + 40f;
			}

            string ename = e.name + "_sign";
            GameObject enemySign = new GameObject(ename);
            Transform EnemySign = enemySign.transform;
            EnemySign.rotation = Camera.main.transform.rotation;
            TextMesh etm = enemySign.AddComponent<TextMesh>();
            etm.text = "[ Text word ]";
            etm.color = new Color(0.8f, 0.8f, 0.8f);
            etm.fontStyle = FontStyle.Bold;
            etm.alignment = TextAlignment.Center;
            etm.anchor = TextAnchor.MiddleCenter;
            etm.characterSize = 0.065f;
            etm.fontSize = 60;
            e.transform.localScale = Vector3.Lerp(e.transform.localScale, Vector3.one, 0.1f);
            EnemySign.position = e.transform.position + Vector3.up * 3f;

            CurrentEnemyNum++;

//			FreshBar();

		}

        public void PowerUp()
        {
            powerUpCount = maxPowerUpCount;
        }

        public void FreshBar3()
        {
            HPBarIMG.transform.localScale = new Vector3((float)timeLeft / 20f, 1f, 1f);
            HPBarTXT.text = string.Format(
                "{0}/{1}  Enemy Here  <color=#ddddddff><size=16>{2} sec/enemy</size></color>",
                timeLeft.ToString("00"),
                "20",
                CurrentSpawnGap.ToString("0.00")
            );
            _1.rectTransform.anchorMin = new Vector2((float)timeLeft / (float)20, 0.5f);
            _1.rectTransform.anchorMax = new Vector2((float)timeLeft / (float)20, 0.5f);
            _1.rectTransform.anchoredPosition = Vector2.down * 20f;
        }

        public void FreshBar2()
        {
            HPBarIMG.transform.localScale = new Vector3((float)currentKillNum / (float)goal, 1f, 1f);
            HPBarTXT.text = string.Format(
                "{0}/{1}  Enemy Here  <color=#ddddddff><size=16>{2} sec/enemy</size></color>",
                currentKillNum.ToString("00"),
                goal.ToString("00"),
                CurrentSpawnGap.ToString("0.00")
            );
            _1.rectTransform.anchorMin = new Vector2((float)currentKillNum / (float)goal, 0.5f);
            _1.rectTransform.anchorMax = new Vector2((float)currentKillNum / (float)goal, 0.5f);
            _1.rectTransform.anchoredPosition = Vector2.down * 20f;
        }

        public void FreshBar () {
			HPBarIMG.transform.localScale = new Vector3((float)CurrentEnemyNum / (float)DeadEnd, 1f, 1f);
			HPBarTXT.text = string.Format(
				"{0}/{1}  Enemy Here  <color=#ddddddff><size=16>{2} sec/enemy</size></color>",
				CurrentEnemyNum.ToString("00"),
				DeadEnd.ToString("00"),
				CurrentSpawnGap.ToString("0.00")
			);
			_1.rectTransform.anchorMin = new Vector2((float)CurrentEnemyNum / (float)DeadEnd, 0.5f);
			_1.rectTransform.anchorMax = new Vector2((float)CurrentEnemyNum / (float)DeadEnd, 0.5f);
			_1.rectTransform.anchoredPosition = Vector2.down * 20f;
		}

        public int addWeaponPowerboy1 = 0;
        public int addWeaponPowerboy2 = 0;
        public int addWeaponPowerbig1 = 0;
        public int addWeaponPowerbig2 = 0;
        public int addWeaponPowerBlack1 = 0;
        public int addWeaponPowerBlack2 = 0;


        public int GetWeaponPower1()
        {
            int weaponPower1 = 0;
            if (Players[currentPlayerID].gameObject.name.Contains("boy"))
            {
                weaponPower1 = addWeaponPowerboy1;
            }
            else if (Players[currentPlayerID].gameObject.name.Contains("GuuGeer"))
            {
                weaponPower1 = addWeaponPowerbig1;
            }
            else if (Players[currentPlayerID].gameObject.name.Contains("Black"))
            {
                weaponPower1 = addWeaponPowerBlack1;
            }

            return weaponPower1;
        }

        public int GetWeaponPower2()
        {
            int weaponPower2 = 0;
            if (Players[currentPlayerID].gameObject.name.Contains("boy"))
            {
                weaponPower2 = addWeaponPowerboy1;
            }
            else if (Players[currentPlayerID].gameObject.name.Contains("GuuGeer"))
            {
                weaponPower2 = addWeaponPowerbig1;
            }
            else if (Players[currentPlayerID].gameObject.name.Contains("Black"))
            {
                weaponPower2 = addWeaponPowerBlack1;
            }

            return weaponPower2;
        }

        public void AddWeaponPower1()
        {
            print(Players[currentPlayerID].gameObject.name);
            if (Players[currentPlayerID].gameObject.name.Contains("Boy"))
            {
                print("boy power up");
                addWeaponPowerboy1 += 2;
            }
            else if (Players[currentPlayerID].gameObject.name.Contains("GuuGeer"))
            {
                print("GuuGeer power up");
                addWeaponPowerbig1 += 2;
            }
            else if (Players[currentPlayerID].gameObject.name.Contains("Black"))
            {
                print("Black power up");
                addWeaponPowerBlack1 += 2;
            }

            maxPowerUpCount++;
        }

        public void AddWeaponPower2()
        {
            if (Players[currentPlayerID].gameObject.name.Contains("Boy"))
            {
                addWeaponPowerboy2 += 5;
            }
            else if (Players[currentPlayerID].gameObject.name.Contains("GuuGeer"))
            {
                addWeaponPowerbig2 += 5;
            }
            else if (Players[currentPlayerID].gameObject.name.Contains("Black"))
            {
                addWeaponPowerBlack2 += 5;
            }

            maxPowerUpCount++;
        }


        public static void AddKillNum (int score) {
			Main.currentKillNum += score;
			if (Main.currentKillNum > Main.highScore) {
				Main.highScore = Main.currentKillNum;
				Vector3 sclh = Main.HighScore.transform.localScale * 4f;
				if (sclh.x > 6f) {
					sclh = Vector3.one * 6f;
				}
				Main.HighScore.transform.localScale = sclh;
//				Main.HighScore.text = Main.highScore.ToString("00");
				PlayerPrefs.SetInt("MoenenVoxel.HighScore", Main.highScore);
			}
			Vector3 scl = Main.KillNum.transform.localScale * 4f;
			if (scl.x > 6f) {
				scl = Vector3.one * 6f;
			}
			Main.combo++;
			Main.KillNum.transform.localScale = scl;
			Main.KillNum.text = Main.currentKillNum.ToString("00");
			Main._1.transform.localScale = Vector3.one * 3f;
			Main._1.text = "<size=32><b>-</b></size> " + Main.combo.ToString();
        }

		public static void UpdateMessage () {
			Main.Msg.text = "Success!";
		}


		public static void PlaySound (int id, float v = 1f) {
			AudioSource.PlayClipAtPoint(DemoStage.Main.SFXs[id], Vector3.zero, v);
		}


		public void PlayDieoutSound () {
			PlaySound(7);
		}



		public void OpenURL (string url) {
			Application.OpenURL(url);
		}


	}
}