//***************************************************************************************
// Writer: Stylish Esper
//***************************************************************************************

using Esper.FeelSpeak.Graph;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Esper.FeelSpeak.UI
{
    /// <summary>
    /// Represents a character in dialogue.
    /// </summary>
    public class DialogueSpeakerUGUI : DialogueSpeaker
    {
        /// <summary>
        /// The label that displays the speaker's name.
        /// </summary>
        public TextMeshProUGUI nameLabel;

        /// <summary>
        /// The image that displays the speaker character's sprite.
        /// </summary>
        public Image characterImage;

        public override void SetActive(Color? color = null)
        {
            nameLabel.gameObject.SetActive(true);

            if (characterImage)
            {
                if (!color.HasValue)
                {
                    color = FeelSpeak.Settings.activeSpeakerSpriteColor;
                }

                characterImage.color = color.Value;
                characterImage.transform.SetAsLastSibling();
            }
        }

        public override void SetInactive(Color? color = null)
        {
            nameLabel.gameObject.SetActive(false);

            if (characterImage)
            {
                if (!color.HasValue)
                {
                    color = FeelSpeak.Settings.inactiveSpeakerSpriteColor;
                }

                characterImage.color = color.Value;
                characterImage.transform.SetAsFirstSibling();
            }
        }

        public override void SetData(DialogueNode dialogueNode)
        {
            if (dialogueNode == null)
            {
                speaker = null;
                currentEmotion = null;
            }
            else
            {
                speaker = FeelSpeak.FindSpeaker(dialogueNode.character);
                currentEmotion = dialogueNode.emotion;
            }

            UpdateElements();
        }

        public override void UpdateElements()
        {
            if (!speaker)
            {
                nameLabel.gameObject.SetActive(false);

                if (characterImage)
                {
                    characterImage.gameObject.SetActive(false);
                }
            }
            else
            {
                var isActive = IsActive();
                nameLabel.gameObject.SetActive(isActive);

                if (FeelSpeak.Settings.enableObjectTextColors)
                {
                    nameLabel.text = $"<color=#{speaker.character.textColor.ToHexString()}>{speaker.character.characterName}</color>";
                }
                else
                {
                    nameLabel.text = $"{speaker.character.characterName}";
                }

                var sprite = speaker.character.GetSprite(currentEmotion);

                if (characterImage)
                {
                    characterImage.gameObject.SetActive(sprite);
                    characterImage.sprite = sprite;
                }

                if (isActive)
                {
                    SetActive();
                }
                else
                {
                    SetInactive();
                }
            }
        }
    }
}