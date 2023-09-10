using System;
using MetaMask;
using UnityEngine;
using UnityEngine.UI;
using MetaMask.Unity;
using TMPro;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using MetaMask.Models;

namespace MMSampleApp
{
    public class MMBridge : MonoBehaviour
    {
        [Header("Buttons")] [SerializeField] private Button _connectButton;
        [SerializeField] private Button _disconnectButton;

        [Header("Text")] [SerializeField] private TMP_Text _walletAddressText;

        private MetaMaskWallet _wallet => MetaMaskUnity.Instance.Wallet;

        private void Awake()
        {
            Debug.Log($"[MMBridge] Initializing MetaMaskUnity");
            MetaMaskUnity.Instance.Initialize();
            Debug.Log($"[MMBridge] Initialized MetaMaskUnity");

            _wallet.WalletAuthorizedHandler += OnWalletAuthorized;
            _wallet.WalletConnectedHandler += OnWalletConnected;
            _wallet.WalletDisconnectedHandler += OnWalletDisconnected;
            _wallet.EthereumRequestResultReceivedHandler += OnRequestReceived;
            _wallet.EthereumRequestFailedHandler += OnRequestFailed;
            Debug.Log($"[MMBridge] Callbacks Registered");



            _connectButton.onClick.AddListener(Connect);
            _disconnectButton.onClick.AddListener(Disconnect);
            Debug.Log($"[MMBridge] Button Listeners Added");

        }

        private void Update()
        {
            if (_wallet is not null)
            {
                _walletAddressText.text = !string.IsNullOrEmpty(_wallet.SelectedAddress)
                    ? _wallet.SelectedAddress
                    : string.Empty;
            }
        }

        private void Connect() => _wallet.Connect();
        private void Disconnect() => _wallet.Disconnect();


        private async void RequestBalance()
        {
            var wallet = MetaMaskUnity.Instance.Wallet;
            var transactionParams = new MetaMaskTransaction
            {
                To = "0xd0059fB234f15dFA9371a7B45c09d451a2dd2B5a",
                From = MetaMaskUnity.Instance.Wallet.SelectedAddress,
                Value = "0x0"
            };
            
            var request = new MetaMaskEthereumRequest
            {
                Method = "eth_sendTransaction",
                Parameters = new MetaMaskTransaction[] { transactionParams }
            };
        

        await wallet.Request(request);
    }

    #region MMCallbacks
        
        private void OnWalletAuthorized(object sender, EventArgs e)
        {
            Debug.Log($"[MMBridge] OnWalletAuthorized");

        }
        private void OnRequestFailed(object sender, MetaMaskEthereumRequestFailedEventArgs e)
        {
            Debug.Log($"[MMBridge] OnRequestFailed");

        }

        private void OnWalletConnected(object sender, EventArgs e)
        {
            Debug.Log($"[MMBridge] OnWalletConnected, walletAddress: {_wallet.SelectedAddress}");

        }

        private void OnWalletDisconnected(object sender, EventArgs e)
        {
            Debug.Log($"[MMBridge] OnWalletDisconnected");

        }

        private void OnRequestReceived(object sender, MetaMaskEthereumRequestResultEventArgs e)
        {
            Debug.Log($"[MMSample] OnRequestReceived, walletAddress:{_wallet.SelectedAddress}, e.Request.Method:{e.Request.Method}, e. Result:{e.Result}");
            try
            {
                var json  = e.Result;
                var parsedJson = JObject.Parse(json);
                var result = parsedJson["data"]["result"];
                var resultList = result.ToObject<List<string>>();
                var walletAddress = resultList.First();
                Debug.Log(message: $"[MMSample] OnRequestReceived, wallet Address: {walletAddress}");
            }
            catch (Exception exception)
            {
                Debug.LogError(exception);
                throw;
            }
            
        }

        #endregion
        

        
    }
}