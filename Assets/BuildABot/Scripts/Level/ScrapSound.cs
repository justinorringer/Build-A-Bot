using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BuildABot {
    public class ScrapSound : MonoBehaviour
    {
        [SerializeField] private GameObject bipy;
        private PlayerMovement _bipyMov;

        private AudioSource _source;

        // Start is called before the first frame update
        void Start()
        {
            _bipyMov = bipy.GetComponent<PlayerMovement>();
            _source = GetComponent<AudioSource>();
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if(collision.gameObject == bipy && _bipyMov.InMotion)
            {
                _source.Play();
            }
        }

        private void OnTriggerStay2D(Collider2D collision)
        {
            if (_source.isPlaying && collision.gameObject == bipy && !_bipyMov.InMotion)
            {
                _source.Pause();
            }
            else if(!_source.isPlaying && collision.gameObject == bipy && _bipyMov.InMotion)
            {
                _source.Play();
            }
        }

        private void OnTriggerExit2D(Collider2D collision)
        {
            if (collision.gameObject == bipy)
            {
                _source.Stop();
            }
        }
    }
}
