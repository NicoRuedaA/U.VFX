using System;
using UnityEngine;
namespace EmeraldArena.Vfx
{
    public sealed class EmeraldVfxGallery : MonoBehaviour
    {
        public EmeraldVfxPlayer player;
        public TextAsset[] clips;
        public string[] moveNames;
        public TextMesh title;
        [Range(1,165)] public int moveId = 53;
        public bool autoAdvance = true;
        [Min(2.1f)] public float interval = 3;
        private int current = -1;
        private float clock;
        private void Update()
        {
            if (clips == null || clips.Length == 0) return;
            clock += Time.deltaTime;
            if (autoAdvance && clock >= interval) { clock = 0; moveId = moveId % clips.Length + 1; }
            int index = Mathf.Clamp(moveId - 1, 0, clips.Length - 1);
            if (index == current) return;
            current = index; player.clip = clips[index]; player.Play();
            if (title) title.text = (index + 1).ToString("000") + "  " + moveNames[index];
        }
    }
}
