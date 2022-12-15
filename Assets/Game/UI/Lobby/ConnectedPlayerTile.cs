using System;
using Mirror;
using Mirror.FizzySteam;
using Steamworks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TonyDev.Game.UI.Lobby
{
    public class ConnectedPlayerTile : NetworkBehaviour
    {
        [SyncVar(hook = nameof(UsernameHook))] public string username;

        [NonSerialized] [SyncVar(hook = nameof(IDHook))]
        public ulong SteamID;

        public TMP_InputField usernameTextField;
        public Image pfpImage;

        public void Awake()
        {
            usernameTextField.readOnly = true;
        }

        public override void OnStartAuthority()
        {
            if (FindObjectOfType<FizzySteamworks>() == null)
            {
                usernameTextField.readOnly = false;
                usernameTextField.onEndEdit.AddListener(FinishEditUsername);
            }
            else
            {
                CmdSetId(SteamUser.GetSteamID().m_SteamID);
                CmdUpdateUsername(SteamFriends.GetPersonaName());
            }
        }

        // [TargetRpc]
        // public void TargetRequestData(NetworkConnection target)
        // {
        //     CmdSetId(SteamUser.GetSteamID().m_SteamID);
        //     CmdUpdateUsername(SteamFriends.GetPersonaName());
        // }

        [Command(requiresAuthority = false)]
        public void CmdSetId(ulong id)
        {
            SteamID = id;
        }

        private void UsernameHook(string oldValue, string newValue)
        {
            usernameTextField.text = newValue;
        }

        private void IDHook(ulong oldValue, ulong newValue)
        {
            pfpImage.sprite = GetPlayerImage(newValue);
        }

        private static Sprite GetPlayerImage(ulong id)
        {
            var avatarHandle = SteamFriends.GetMediumFriendAvatar(new CSteamID(id));
            return GetSteamImageAsTexture(avatarHandle);
        }

        private static Sprite GetSteamImageAsTexture(int iImage)
        {
            Texture2D texture = null;

            bool isValid = SteamUtils.GetImageSize(iImage, out uint width, out uint height);
            if (isValid)
            {
                byte[] image = new byte[width * height * 4];

                isValid = SteamUtils.GetImageRGBA(iImage, image, (int) (width * height * 4));

                if (isValid)
                {
                    texture = new Texture2D((int) width, (int) height, TextureFormat.RGBA32, false, true);
                    texture.LoadRawTextureData(image);
                    texture.Apply();
                }
            }

            return texture == null ? null : Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
        }

        private void FinishEditUsername(string newUsername)
        {
            if (hasAuthority)
            {
                CmdUpdateUsername(newUsername);
            }
        }

        [Command(requiresAuthority = false)]
        private void CmdUpdateUsername(string newUsername)
        {
            username = newUsername;
        }
    }
}