using System.Collections;
using System.Linq; // for characterData.Name lookup
using UnityEngine; // for Debug

namespace UGESystem
{
        /// <summary>
        /// Command handler that executes a <see cref="DialogueCommand"/> by displaying text and character names
        /// via the <see cref="UGEUIManager"/>, and waits for user input to proceed.
        /// </summary>
        public class DialogueNode_DialogueCommandHandler : ICommandHandler
        {
            /// <summary>
            /// Executes the <see cref="DialogueCommand"/>. It handles character display logic and
            /// requests the <see cref="UGEUIManager"/> to show the dialogue, then waits for user input.
            /// </summary>
            /// <param name="genericCommand">The command to execute, expected to be a <see cref="DialogueCommand"/>.</param>
            /// <param name="controller">The <see cref="UGEGameEventController"/> managing the current game event flow.</param>
            /// <returns>An IEnumerator for coroutine execution.</returns>
            public IEnumerator Execute(IGameEventCommand genericCommand, UGEGameEventController controller)
            {
                DialogueCommand command = (DialogueCommand)genericCommand; // Explicit cast
    
                // 1. 캐릭터 관련 로직을 CharacterManager의 헬퍼 메서드로 위임
                controller.CharacterManager.ShowCharacterForDialogue(command);
    
                // 2. UIManager에 대사 출력을 요청
                string displayName = command.CharacterName;
                if (controller.CharacterManager.CharacterDB != null)
                {
                    CharacterData data = controller.CharacterManager.CharacterDB.GetCharacterData(command.CharacterName);
                    if (data != null && !string.IsNullOrEmpty(data.Name))
                    {
                        displayName = data.Name;
                    }
                }
                controller.UIManager.ShowDialogue(displayName, command.DialogueText);
    
                controller.IsWaitingForChoice = true; // 사용자의 입력을 기다리도록 설정 // Set to wait for user input
                yield break;
            }
        }
    }
    