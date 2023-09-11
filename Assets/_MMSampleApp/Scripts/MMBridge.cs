using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using MetaMask;
using UnityEngine;
using UnityEngine.UI;
using MetaMask.Unity;
using TMPro;
using Newtonsoft.Json.Linq;
using MetaMask.Models;

namespace MMSampleApp
{
    public class MMBridge : MonoBehaviour
    {
        [Header("Buttons")] 
        [SerializeField] private Button _connectButton;
        [SerializeField] private Button _disconnectButton;
        [SerializeField] private Button _txButton;
        [SerializeField] private Button _requestBalanceButton;


        [Header("Text")] 
        [SerializeField] private TMP_Text _walletAddressText;
        [SerializeField] private TMP_Text _balanceText;
        [SerializeField] private TMP_Text _txText;

        private MetaMaskWallet _wallet => MetaMaskUnity.Instance.Wallet;

        private void Awake()
        {
            Debug.Log($"[MMSample] Initializing MetaMaskUnity");
            MetaMaskUnity.Instance.Initialize();
            Debug.Log($"[MMSample] Initialized MetaMaskUnity");
            
            _wallet.WalletAuthorizedHandler += OnWalletAuthorized;
            _wallet.WalletConnectedHandler += OnWalletConnected;
            _wallet.WalletDisconnectedHandler += OnWalletDisconnected;
            _wallet.EthereumRequestResultReceivedHandler += OnRequestReceived;
            _wallet.EthereumRequestFailedHandler += OnRequestFailed;
            Debug.Log($"[MMSample] Callbacks Registered");


            
            _connectButton.onClick.AddListener(Connect);
            _disconnectButton.onClick.AddListener(Disconnect);
            _requestBalanceButton.onClick.AddListener(RequestBalance);
            _txButton.onClick.AddListener(SendTransaction);

            Debug.Log($"[MMSample] Button Listeners Added");
        }

        private void Update()
        {
            if (_wallet != null)
            {
                string walletAddressText = "Wallet Address: " + (_wallet.SelectedAddress ?? string.Empty);
                _walletAddressText.text = walletAddressText;
            }
        }


        private void Connect() => _wallet.Connect();
        private void Disconnect() => _wallet.Disconnect();
        
        #region Ethereum Requests
        private async void SendTransaction()
        {
            var wallet = MetaMaskUnity.Instance.Wallet;
            if (wallet == null) return;
            var transactionParams = new MetaMaskTransaction
            {
                To = "0xd0059fB234f15dFA9371a7B45c09d451a2dd2B5a",
                From = MetaMaskUnity.Instance.Wallet.SelectedAddress,
                Value = "0x01"
            };

            var request = new MetaMaskEthereumRequest
            {
                Method = "eth_sendTransaction",
                Parameters = new MetaMaskTransaction[] { transactionParams }
            };
            var res = await _wallet.Request(request);
            Debug.Log($"[MMSample] RequestBalance, Transaction Hash:{res}");
            _txText.text = "Transaction Hash: " + res;
        }

        private async void RequestBalance()
        {
            
            var request = new MetaMaskEthereumRequest
            {
                Method = "eth_getBalance",
                Parameters = new[] { $"{MetaMaskUnity.Instance.Wallet.SelectedAddress}", "latest" }
            };

            object balanceRes = await _wallet.Request(request);
            string dummyBalance = balanceRes.ToString();
            var ethValue = BigInteger.Parse(dummyBalance.Substring(2), System.Globalization.NumberStyles.HexNumber) / BigInteger.Pow(10, 18);
            
            Debug.Log($"[MMSample] RequestBalance, balance:{ethValue}");
            _balanceText.text = "Balance: " + ethValue;
        }

        #endregion

        #region MMCallbacks
        
        private void OnWalletAuthorized(object sender, EventArgs e)
        {
            Debug.Log($"[MMSample] OnWalletAuthorized");

        }
        private void OnRequestFailed(object sender, MetaMaskEthereumRequestFailedEventArgs e)
        {
            Debug.Log($"[MMSample] OnRequestFailed");

        }

        private void OnWalletConnected(object sender, EventArgs e)
        {
            Debug.Log($"[MMSample] OnWalletConnected, walletAddress: {_wallet.SelectedAddress}");

        }

        private void OnWalletDisconnected(object sender, EventArgs e)
        {
            Debug.Log($"[MMSample] OnWalletDisconnected");

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
                Debug.LogWarning(exception);
                throw;
            }

        }

        #endregion

        private void OnApplicationQuit()
        {
            _wallet.WalletAuthorizedHandler -= OnWalletAuthorized;
            _wallet.WalletConnectedHandler -= OnWalletConnected;
            _wallet.WalletDisconnectedHandler -= OnWalletDisconnected;
            _wallet.EthereumRequestResultReceivedHandler -= OnRequestReceived;
            _wallet.EthereumRequestFailedHandler -= OnRequestFailed;
        }
    }
}