using System;
using UnityEngine;
using System.Collections;
using LEGO.Logger;

namespace LEGODeviceUnitySDK
{


    internal class ConnectInterrogationTimer 
    {
        #region Constants guiding the interrogation process
        private const float MINIMUM_INTERROGATION_TIME = 3f;
        private const float MAX_TIME_TO_WAIT_FOR_NEXT_PACKAGE_DURING_INTERROGATION = 2.5f;
        public enum InterrogationState
        {
            InProgress, 
            Complete, 
            Failed 
        }
        #endregion

        public InterrogationState CurrentState 
        {
            get;
            private set;
        }
        
        
        private static readonly ILog logger = LogManager.GetLogger(typeof(ConnectInterrogationTimer));

        private readonly MonoBehaviour coroutineHelper;

        private Func<bool> servicesAreFullyKnown;

        #region Timers & state
        private bool gotHubAlerts = false;
        private bool gotHubProperties = false;
        private bool minimumTimePassed = false;
        private bool done = false;
        private readonly Action<bool> onCompletion;
        private Coroutine globalTimer, localTimer;
        #endregion

        public ConnectInterrogationTimer(MonoBehaviour coroutineHelper, Func<bool> servicesAreFullyKnown, Action<bool> onCompletion)
        {
            logger.Debug("ConnectInterrogationTimer created");
            this.coroutineHelper = coroutineHelper;
            this.onCompletion = onCompletion;
            this.servicesAreFullyKnown = servicesAreFullyKnown;
            
            CurrentState = InterrogationState.InProgress;
            
            StartGlobalTimer();
        }

        #region Events from outside

        public void Restart()
        {
            done = false;
            minimumTimePassed = false;
            
            StopTimers();
            
            CurrentState = InterrogationState.InProgress;
            
            StartGlobalTimer();
        }
        
        public void Cancel() 
        {
            AbortInterrogation("Cancelled");
        }
        
        public void GotHubAlerts()
        {
            gotHubAlerts = true;
            MajorProgressTick();
        }
        public void GotHubProperties()
        {
            gotHubProperties = true;
            MajorProgressTick();
        }

        public void MinorProgressTick() 
        {
            RestartLocalTimer();
        }

        public void MajorProgressTick() 
        {
            CheckWhetherDone();
            if (!done) 
                RestartLocalTimer();
        }
        #endregion

        #region Timers
        private void StartGlobalTimer()
        {
            globalTimer = coroutineHelper.StartCoroutine(GlobalTimer());
        }

        private void RestartLocalTimer()
        {
            if (localTimer != null) 
                coroutineHelper.StopCoroutine(localTimer);
            localTimer = coroutineHelper.StartCoroutine(LocalTimer());
        }

        private void StopTimers() 
        {
            if (globalTimer != null) 
            {
                coroutineHelper.StopCoroutine(globalTimer);
                globalTimer = null;
            }
            if (localTimer != null) 
            {
                coroutineHelper.StopCoroutine(localTimer);
                localTimer = null;
            }
        }

        private IEnumerator GlobalTimer() 
        {
            yield return new WaitForSeconds(MINIMUM_INTERROGATION_TIME);
            logger.Debug("Interrogation minimum-timer triggered");
            minimumTimePassed = true;
            if(!gotHubAlerts)
            {
                AbortInterrogation("Hub alerts missing at global timeout");
            }
            else if (!gotHubProperties) 
            {
                AbortInterrogation("Hub properties missing at global timeout");
            }
            else 
            {
                MajorProgressTick();
            }
        }

        private IEnumerator LocalTimer() 
        {
            yield return new WaitForSeconds(MAX_TIME_TO_WAIT_FOR_NEXT_PACKAGE_DURING_INTERROGATION);
            logger.Debug("Interrogation idle-timer triggered");
            // Either we trigger completion, or we abort:
            minimumTimePassed = true;
            MajorProgressTick();
            if (!done) 
            {
                AbortInterrogation("Idle timeout.");
            }
        }
        #endregion

        private void AbortInterrogation(string reason)
        {
            logger.Warn("Interrogation aborted: "+reason);
            TerminateInterrogation(false);
        }

        private void TerminateInterrogation(bool success)
        {
            if (done) { // Paranoia
                logger.Error("Double done! "+success);
                return;
            }

            CurrentState = success ? InterrogationState.Complete : InterrogationState.Failed;
            
            StopTimers();
            done = true;
            
            onCompletion(success);
        }

        private void CheckWhetherDone()
        {
            if (   minimumTimePassed 
                && gotHubAlerts 
                && gotHubProperties 
                && servicesAreFullyKnown()
                && CurrentState == InterrogationState.InProgress) 
            {
                logger.Info("Interrogation completed.");
                TerminateInterrogation(true);
            }
        }

    }

    /**********************************************************
     * PURPOSE: Controlling the interrogation flow.           *
     **********************************************************/
    
}
