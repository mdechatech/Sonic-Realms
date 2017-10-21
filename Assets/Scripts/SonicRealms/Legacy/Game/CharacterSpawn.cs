using UnityEngine;

namespace SonicRealms.Legacy.Game
{
    public class CharacterSpawn : MonoBehaviour
    {
        public virtual GameObject Spawn(CharacterData character)
        {
            return Spawn(character, gameObject);
        }

        public virtual GameObject Spawn(CharacterData character, GameObject checkpoint)
        {
            var newCharacter = Instantiate(character.PlayerObject);
            newCharacter.name = character.name;
            newCharacter.transform.position = checkpoint.transform.position;
            return newCharacter;
        }
    }
}
