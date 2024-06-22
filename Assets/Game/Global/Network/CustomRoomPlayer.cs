using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Mirror;
using Steamworks;
using TMPro;
using TonyDev.Game.Core.Effects;
using TonyDev.Game.Core.Entities.Player;
using TonyDev.Game.Level;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace TonyDev.Game.Global.Network
{
    public class CustomRoomPlayer : NetworkRoomPlayer
    {
        public static CustomRoomPlayer Local;

        [SyncVar(hook = nameof(OnSteamIDChange))]
        public ulong steamID;

        [SyncVar(hook = nameof(OnUsernameChange))]
        public string username;

        [SyncVar(hook = nameof(OnSkinChanged))]
        public PlayerSkin skin;

        [SyncVar] public string classEffectName;
        
        public readonly SyncList<string> UnlockedItemNames = new();

        [Header("UI")] public Transform uiTransform;
        public Button actionButton;
        public TMP_Text actionButtonText;
        public TMP_Text usernameText;
        public TMP_Text statusText;
        public Image profileImage;
        public Image[] playerImages;

        public Button skinLeft;
        public Button skinRight;

        private int _skinIndex = 0;
        
        private void OnSkinChanged(PlayerSkin oldSkin, PlayerSkin newSkin)
        {
            foreach (var playerImage in playerImages)
            {
                playerImage.material = new Material(playerImage.material);

                playerImage.material.SetColor("_LowColor", newSkin.LowColor);
                playerImage.material.SetColor("_MidColor", newSkin.MidColor);
                playerImage.material.SetColor("_HighColor", newSkin.HighColor);
            }
        }
        
        [Command(requiresAuthority = false)]
        public void CmdSetClassFxName(string newName)
        {
            classEffectName = newName;
        }
        
        [Command]
        public void CmdSetUnlockedItems(string[] items)
        {
            foreach (var i in items)
            {
                UnlockedItemNames.Add(i);
            }
        }
        
        [Command(requiresAuthority = false)]
        public void CmdSetSkin(PlayerSkin newSkin)
        {
            skin = newSkin;
        }

        public bool UIEnabled => SceneManager.GetActiveScene().name == "LobbyScene";

        public void OnSteamIDChange(ulong oldId, ulong newId)
        {
            if (newId == oldId || !UIEnabled) return;
            profileImage.sprite = GetSteamImageAsTexture(newId);
        }

        public void OnUsernameChange(string oldUser, string newUser)
        {
            if (!UIEnabled) return;
            usernameText.text = newUser;
        }

        public override void OnStartAuthority()
        {
            Local = this;

            CmdSetSteamId(SteamUser.GetSteamID().m_SteamID);
            CmdSetUsername(SteamFriends.GetPersonaName());

            CmdSetUnlockedItems(UnlocksManager.Instance.Unlocks.ToArray());

            if (!UIEnabled) return;

            _skinIndex = PlayerPrefs.GetInt("skin");
            CmdSetSkin(PlayerSkin.skins[_skinIndex]);

            actionButtonText.text = "Ready";

            if (isServer)
            {
                CmdChangeReadyState(true);
            }

            actionButton.onClick.AddListener(() =>
            {
                CmdChangeReadyState(!readyToBegin);
            });
            
            skinRight.onClick.AddListener(() =>
            {
                _skinIndex += 1;
                if(_skinIndex >= PlayerSkin.skins.Length) _skinIndex = 0;
                PlayerPrefs.SetInt("skin", _skinIndex);
                PlayerPrefs.Save();
                CmdSetSkin(PlayerSkin.skins[_skinIndex]);
            });
            
            skinLeft.onClick.AddListener(() =>
            {
                _skinIndex -= 1;
                if(_skinIndex < 0) _skinIndex = PlayerSkin.skins.Length-1;
                PlayerPrefs.SetInt("skin", _skinIndex);
                PlayerPrefs.Save();
                CmdSetSkin(PlayerSkin.skins[_skinIndex]);
            });
        }

        public override void OnStartServer()
        {
            if (!UIEnabled) return;

            if (!isOwned)
            {
                actionButtonText.text = "Kick";
                actionButton.onClick.AddListener(KickPlayer);
            }
        }

        public override void OnStartClient()
        {
            if (!UIEnabled) return;

            FindObjectOfType<LobbyUI>().AddPlayerObject(uiTransform);

            if (!isOwned && !isServer) actionButton.gameObject.SetActive(false);
            if (!isOwned)
            {
                skinRight.gameObject.SetActive(false);
                skinLeft.gameObject.SetActive(false);
            }
        }

        public override void ReadyStateChanged(bool oldReadyState, bool newReadyState)
        {
            base.ReadyStateChanged(oldReadyState, newReadyState);

            if (!UIEnabled) return;

            statusText.text = newReadyState ? "Ready" : "Not Ready";
            statusText.color = newReadyState ? Color.green : Color.red;

            if (!isOwned) return;
            
            actionButtonText.text = newReadyState ? "Unready" : "Ready";
        }

        [Server]
        private void KickPlayer()
        {
            netIdentity.connectionToClient.Disconnect();
        }

        [Command]
        public void CmdSetSteamId(ulong newId)
        {
            steamID = newId;
        }

        [Command]
        private void CmdSetUsername(string newUser)
        {
            username = newUser;
        }

        [ClientRpc]
        public void RpcStartFadeOut()
        {
            StartFadeTask().Forget();
        }

        private async UniTask StartFadeTask()
        {
            TransitionController.Instance.FadeOut();
            await UniTask.Delay(TimeSpan.FromSeconds(TransitionController.FadeOutTimeSeconds));
        }

        private void OnDestroy()
        {
            if (isOwned) Local = null;

            if(uiTransform != null) Destroy(uiTransform.gameObject);
        }

        private static Sprite GetSteamImageAsTexture(ulong id)
        {
            var avatarHandle = SteamFriends.GetMediumFriendAvatar(new CSteamID(id));

            Texture2D texture = null;

            bool isValid = SteamUtils.GetImageSize(avatarHandle, out uint width, out uint height);
            if (isValid)
            {
                byte[] image = new byte[width * height * 4];

                isValid = SteamUtils.GetImageRGBA(avatarHandle, image, (int) (width * height * 4));

                if (isValid)
                {
                    texture = new Texture2D((int) width, (int) height, TextureFormat.RGBA32, false, true);
                    texture.LoadRawTextureData(image);
                    texture.Apply();
                }
            }

            return texture == null
                ? null
                : Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
        }
    }
}