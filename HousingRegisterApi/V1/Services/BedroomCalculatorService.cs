using HousingRegisterApi.V1.Domain;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HousingRegisterApi.V1.Services
{
    public class BedroomCalculatorService : IBedroomCalculatorService
    {
        /// <summary>
        /// Calculates the required number of bedrooms
        /// </summary>
        /// <param name="application"></param>
        /// <returns></returns>
        /// <exception cref="ApplicationException"></exception>
        public int Calculate(Application application)
        {
            ValidateApplication(application);

            IEnumerable<Applicant> household = new List<Applicant> { application.MainApplicant }.Concat(application.OtherMembers);

            // combine all applicants into one collection           
            var people = household.ToList().Select(x => x.Person);

            // create a list of available occupants
            // we will use this to remove occupants that have been allocated rooms
            var unoccupants = people.Select(x => new Occupant(x.Age, x.Gender, x.RelationshipType)).ToList();

            // rules must be applied in order
            var count = 0;
            count += ApplyRule1(unoccupants);
            count += ApplyRule2(unoccupants);
            count += ApplyRule3(unoccupants);
            count += ApplyRule4(unoccupants);
            count += ApplyRule5(unoccupants);

            return count;
        }

        private static void ValidateApplication(Application application)
        {
            if (application == null)
            {
                throw new ApplicationException("Application not found");
            }

            // make sure data is valid
            if (application.MainApplicant == null)
            {
                throw new ApplicationException("Main applicant is missing");
            }

            application.OtherMembers.ToList().ForEach(app =>
            {
                if (app == null)
                {
                    throw new ApplicationException("OtherMember is missing");
                }

                if (app.Person == null)
                {
                    throw new ApplicationException("OtherMember Person is missing");
                }
            });
        }

        /// <summary>
        /// One room for each couple who are married, in a civil partnership or otherwise cohabiting who live in your household
        /// </summary>
        /// <param name="unoccupants"></param>
        /// <returns></returns>
        private static int ApplyRule1(List<Occupant> unoccupants)
        {
            var predicate = new Predicate<Occupant>(x => string.IsNullOrWhiteSpace(x.RelationshipType) || x.RelationshipType == "partner");
            var rule = unoccupants.FindAll(predicate);
            // pair up and return biggest int
            var ruleCount = Convert.ToInt32(Math.Ceiling(Decimal.Divide(rule.Count, 2)));
            unoccupants.RemoveAll(predicate);
            return ruleCount;
        }

        /// <summary>
        /// One room for each single person over 21 in your household
        /// </summary>
        /// <param name="unoccupants"></param>
        /// <returns></returns>
        private static int ApplyRule2(List<Occupant> unoccupants)
        {
            var predicate = new Predicate<Occupant>(x => x.Age >= 21);
            var rule = unoccupants.FindAll(predicate);
            var ruleCount = rule.Count;
            unoccupants.RemoveAll(predicate);
            return ruleCount;
        }

        /// <summary>
        /// One room for every two persons under 21 of the same sex in your household
        /// </summary>
        /// <param name="unoccupants"></param>
        /// <returns></returns>
        private static int ApplyRule3(List<Occupant> unoccupants)
        {
            // at this point only people under 21 should only exist in the household
            var ruleCount = 0;
            var under21ByGender = unoccupants.GroupBy(x => x.Gender);

            foreach (var under21ByGenderGroup in under21ByGender)
            {
                var peopleInGroup = under21ByGenderGroup.ToList();

                // loop though while only pairs exist
                while (CanPair(peopleInGroup))
                {
                    // take top 2
                    var occupant1 = peopleInGroup.Last();
                    peopleInGroup.RemoveAt(peopleInGroup.Count - 1);
                    unoccupants.RemoveAll(x => x.Id == occupant1.Id);

                    var occupant2 = peopleInGroup.Last();
                    peopleInGroup.RemoveAt(peopleInGroup.Count - 1);
                    unoccupants.RemoveAll(x => x.Id == occupant2.Id);

                    ruleCount++;
                }
            }

            return ruleCount;
        }

        /// <summary>
        /// One room for every two children of opposite sexes, provided each is under the age of 10 in your household
        /// </summary>
        /// <param name="unoccupants"></param>
        /// <returns></returns>
        private static int ApplyRule4(List<Occupant> unoccupants)
        {
            var ruleCount = 0;
            var childrenUnder10 = unoccupants.FindAll(x => x.Age < 10);

            // loop though while only pairs exist
            while (CanPair(childrenUnder10))
            {
                // take top 2
                var child1 = childrenUnder10.Last();
                childrenUnder10.RemoveAt(childrenUnder10.Count - 1);
                unoccupants.RemoveAll(x => x.Id == child1.Id);

                var child2 = childrenUnder10.Last();
                childrenUnder10.RemoveAt(childrenUnder10.Count - 1);
                unoccupants.RemoveAll(x => x.Id == child2.Id);

                ruleCount++;
            }

            return ruleCount;
        }

        /// <summary>
        /// One room for every single person of any age, including an adult child, of either sex in your household
        /// when there is no-one else in the household suitable to share with.
        /// </summary>
        /// <param name="unoccupants"></param>
        /// <returns></returns>
        private static int ApplyRule5(List<Occupant> unoccupants)
        {
            var ruleCount = unoccupants.Count;
            unoccupants.Clear();
            return ruleCount;
        }

        /// <summary>
        /// check if we can create pairs of occupants
        /// </summary>
        /// <param name="occupants"></param>
        /// <returns></returns>
        private static bool CanPair(List<Occupant> occupants)
        {
            return occupants.Count >= 2 || (occupants.Any() && occupants.Count % 2 == 0);
        }

        /// <summary>
        /// Houshold Occupant
        /// </summary>
        private class Occupant
        {
            public Guid Id { get; }

            public int Age { get; }

            public string Gender { get; }

            public string RelationshipType { get; }

            public Occupant(int age, string gender, string relationToMainApplicant)
            {
                Id = Guid.NewGuid();
                Age = age;
                Gender = gender;
                RelationshipType = relationToMainApplicant;
            }
        }
    }
}
