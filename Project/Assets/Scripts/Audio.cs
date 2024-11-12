using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Solitaire
{
    public class Audio
    {
        private enum Clip { MoveCard, WasteToStock, Num }

        private static readonly Dictionary<Clip, string> ResourcePath = new Dictionary<Clip, string>()
        {
            { Clip.MoveCard,     "Audio/MoveCard"    },
            { Clip.WasteToStock, "Audio/MoveToStock" },
        };

        private AudioSource _audioSource;
        private AsyncOperationHandle[] _audioClipHandle = new AsyncOperationHandle[(int)Clip.Num];
        private AudioClip[] _audioClip = new AudioClip[(int)Clip.Num];

        public void Init()
        {
            _audioSource = new GameObject().AddComponent<AudioSource>();
            _audioSource.gameObject.name = "Audio";
        }

        public async Task LoadResourcesAsync()
        {
            foreach (var clip in ResourcePath)
            {
                _audioClipHandle[(int)clip.Key] = Addressables.LoadAssetAsync<AudioClip>(clip.Value);
            }
            await Task.WhenAll(_audioClipHandle.Select(handle => handle.Task));

            for (var i = 0; i < _audioClipHandle.Length; ++i)
            {
                _audioClip[i] = _audioClipHandle[i].Result as AudioClip;
            }
        }

        public void ReleaseResources()
        {
            foreach (var clip in ResourcePath)
            {
                Addressables.Release(_audioClipHandle[(int)clip.Key]);
            }
        }

        public void OnMoveOneStep(CardMoveStep cardMoveStep)
        {
            switch (cardMoveStep)
            {
                case CardMoveStep.WasteToStock:
                    _audioSource.PlayOneShot(_audioClip[(int)Clip.WasteToStock]);
                    break;

                case CardMoveStep.StockToWaste:
                case CardMoveStep.WasteToPile:
                case CardMoveStep.WasteToFoundation:
                case CardMoveStep.PileToPile:
                case CardMoveStep.PileToFoundation:
                case CardMoveStep.FoundationToPile:
                    _audioSource.PlayOneShot(_audioClip[(int)Clip.MoveCard]);
                    break;

                case CardMoveStep.FaceupPile:
                    break;

                default:
                    break;
            }
        }

        public void OnDealCards()
        {
            _audioSource.PlayOneShot(_audioClip[(int)Clip.WasteToStock]);
        }
    }
}
