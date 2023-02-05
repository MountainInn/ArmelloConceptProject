using UnityEngine;
using UniRx;
using UniRx.Triggers;
using MountainInn;

public class UniRxTesting : MonoBehaviour
{
    ReactiveProperty<bool>
        firstPhaseCompleted,
        secondPhaseCompleted,
        thirdPhaseCompleted;

    void Awake()
    {
        firstPhaseCompleted = new ReactiveProperty<bool>();
        secondPhaseCompleted = new ReactiveProperty<bool>();
        thirdPhaseCompleted = new ReactiveProperty<bool>();

        {
            var key1Pressed =
                this.UpdateAsObservable()
                .Where(_ => Input.GetKeyDown(KeyCode.Alpha1))
                .Select(_ => true);
            var key2Pressed =
                this.UpdateAsObservable()
                .Where(_ => Input.GetKeyDown(KeyCode.Alpha2))
                .Select(_ => true);
            var key3Pressed =
                this.UpdateAsObservable()
                .Where(_ => Input.GetKeyDown(KeyCode.Alpha3))
                .Select(_ => true);


            key1Pressed
                .Subscribe(_ => firstPhaseCompleted.Value = true);
            key2Pressed
                .Subscribe(_ => secondPhaseCompleted.Value = true);
            key3Pressed
                .Subscribe(_ => thirdPhaseCompleted.Value = true);
        }


        var completed =
            Observable
            .CombineLatest(firstPhaseCompleted,
                           secondPhaseCompleted,
                           thirdPhaseCompleted,
                           BoolExt.All)
            .Where(result => result == true);

        completed
            .Subscribe((_) =>
            {
                Debug.Log("Completed: " + _);
            })
            .AddTo(this);

    }
}
