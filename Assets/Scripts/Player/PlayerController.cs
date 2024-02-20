using UnityEngine;


namespace Player
{
    public class PlayerController : MonoBehaviour
    {
        public static PlayerState PlayerState { get; private set; }
        
        private PlayerSateObserver _observer;

        private bool _canChangeState;


        private void Awake()
        {
            _observer = new PlayerSateObserver(SetPlayerState);
            _observer.Subscribe();
        }

        private void OnDestroy()
        {
            _observer.Unsubscribe();
        }

        private void SetPlayerState(PlayerState playerState)
        {
            if (_canChangeState)
                PlayerState = playerState;
        }
    }
}