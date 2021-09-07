using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static FunctionApp.Models.AttemptsEntityState;
using static FunctionApp.Models.Enums;

namespace FunctionApp.Models
{
    /// <summary>
    /// Interface for AttempsEntity.
    /// </summary>
    public interface IAttemptsEntity
    {
        /// <summary>
        /// Adds a new Attempt to the entity. Uses default values apart from Id.
        /// </summary>
        /// <param name="attemptId">The ID to assign to the Attempt.</param>
        void AddAttempt(Guid attemptId);

        /// <summary>
        /// Updates an exsiting Attempt in entity state.
        /// </summary>
        /// <param name="attempt">The Attempt to update.</param>
        void UpdateAttempt(Attempt attempt);

        /// <summary>
        /// Updates just the State field for an existing Attempt in entity state.
        /// </summary>
        /// <param name="update">KVP where the key is the Id of the Attempt to update and the value is the new State field.</param>
        void UpdateAttemptState(KeyValuePair<Guid, AttemptState> update);

        /// <summary>
        /// Updates just the Message field for an existing Attempt in entity state.
        /// </summary>
        /// <param name="update">KVP where the key is the Id of the Attempt to update and the value is the new StatusText field.</param>
        void UpdateAttemptMessage(KeyValuePair<Guid, string> update);

        /// <summary>
        /// Updates just the TimeoutDue field for an existing Attempt in entity state.
        /// </summary>
        /// <param name="update">KVP where the key is the Id of the Attempt to update and the value is the new TimeoutDue field.</param>
        void UpdateAttemptTimeoutDue(KeyValuePair<Guid, DateTime> update);

        /// <summary>
        /// Update sthe OverallState field in entity state.
        /// </summary>
        /// <param name="newState">The new value for the OverallState field.</param>
        void UpdateOverallState(string newState);

        /// <summary>
        /// Gets entire enity state
        /// </summary>
        /// <returns>A AttemptsEntityState.</returns>
        Task<AttemptsEntityState> Get();
    }
}
