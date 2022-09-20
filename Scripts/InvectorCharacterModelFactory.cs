using Invector.vCharacterController;
using MultiplayerARPG.GameData.Model.Playables;
using UnityEngine;

namespace MultiplayerARPG
{
    public class InvectorCharacterModelFactory : ICharacterModelFactory
    {
        public string Name => "Invector Character Model";
        public DimensionType DimensionType => DimensionType.Dimension3D;

        public InvectorCharacterModelFactory()
        {

        }

        public bool ValidateSourceObject(GameObject obj)
        {
            Animator comp = obj.GetComponentInChildren<Animator>();
            if (comp == null)
            {
                Debug.LogError("Cannot create new entity with `InvectorCharacterModel`, can't find `Animator` component");
                Object.DestroyImmediate(obj);
                return false;
            }
            return true;
        }

        public BaseCharacterModel Setup(GameObject obj)
        {
            vHeadTrack headTrack = obj.GetOrAddComponent<vHeadTrack>();
            InvectorCharacterModel characterModel = obj.AddComponent<InvectorCharacterModel>();
            characterModel.animator = obj.GetComponentInChildren<Animator>();
            characterModel.headTrack = headTrack;
            return characterModel;
        }
    }
}
