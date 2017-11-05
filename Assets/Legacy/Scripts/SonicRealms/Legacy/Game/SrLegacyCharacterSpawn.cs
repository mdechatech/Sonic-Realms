using UnityEngine;

namespace SonicRealms.Legacy.Game
{
    public class SrLegacyCharacterSpawn : MonoBehaviour
    {
        public virtual GameObject Spawn(SrLegacyCharacterData character)
        {
            return Spawn(character, gameObject);
        }

        public virtual GameObject Spawn(SrLegacyCharacterData character, GameObject checkpoint)
        {
            var newCharacter = Instantiate(character.PlayerObject);
            newCharacter.name = character.name;
            newCharacter.transform.position = checkpoint.transform.position;
            return newCharacter;
        }
    }
}
