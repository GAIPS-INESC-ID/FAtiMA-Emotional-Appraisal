﻿using System;
using System.Collections.Generic;
using System.Linq;
using AutobiographicMemory;
using EmotionalAppraisal.Components;
using EmotionalAppraisal.DTOs;
using WellFormedNames;

namespace EmotionalAppraisal.OCCModel
{
	public class OCCAffectDerivationComponent : IAffectDerivator
	{
		/*public const int GOALCONFIRMED = 1;
		public const int GOALUNCONFIRMED = 0;
		public const int GOALDISCONFIRMED = 2;
        */
		private static OCCBaseEmotion OCCAppraiseCompoundEmotions(IBaseEvent evt, float desirability, float praiseworthiness)
		{
			if ((desirability == 0) || (praiseworthiness == 0) || ((desirability > 0) != (praiseworthiness > 0)))
				return null;

			float potential = Math.Abs(desirability + praiseworthiness) * 0.5f;

			Name direction;
			OCCEmotionType emoType;

			if(evt.Subject == Name.SELF_SYMBOL)
			{
				direction = Name.SELF_SYMBOL;
				emoType = (desirability > 0) ? OCCEmotionType.Gratification : OCCEmotionType.Remorse;
			}
			else
			{
				direction = evt.Subject ?? Name.UNIVERSAL_SYMBOL;
				emoType = (desirability > 0) ? OCCEmotionType.Gratitude : OCCEmotionType.Anger;
			}

			return new OCCBaseEmotion(emoType, potential, evt.Id, direction, evt.EventName);
		}

		private static OCCBaseEmotion OCCAppraiseWellBeing(uint evtId, Name eventName, float desirability) {
			if(desirability >= 0)
				return new OCCBaseEmotion(OCCEmotionType.Joy, desirability, evtId, eventName);
			return new OCCBaseEmotion(OCCEmotionType.Distress, -desirability, evtId, eventName);
		}

		private static OCCBaseEmotion OCCAppraiseFortuneOfOthers(IBaseEvent evt, float desirability, float desirabilityForOther, string target)
		{
			float potential = (Math.Abs(desirabilityForOther) + Math.Abs(desirability)) * 0.5f;

            if(target == "SELF" || target == evt.Subject.ToString())
                return OCCAppraiseWellBeing(evt.Id, evt.EventName, potential);

			OCCEmotionType emoType;
			if (desirability >= 0)
				emoType = (desirabilityForOther >= 0) ? OCCEmotionType.HappyFor : OCCEmotionType.Gloating;
			else
				emoType = (desirabilityForOther >= 0) ? OCCEmotionType.Resentment : OCCEmotionType.Pitty;

			return new OCCBaseEmotion(emoType, potential, evt.Id, (Name)target, evt.EventName);
		}

		private static OCCBaseEmotion OCCAppraisePraiseworthiness(IBaseEvent evt, float praiseworthiness, string target) {
			Name direction;
			OCCEmotionType emoType;

			if (target == "SELF" || target == evt.Subject.ToString())
			{
				direction = Name.SELF_SYMBOL;
				emoType = (praiseworthiness >= 0) ? OCCEmotionType.Pride : OCCEmotionType.Shame;
			}
			else
			{
               direction = (Name)target;
				emoType = (praiseworthiness >= 0) ? OCCEmotionType.Admiration : OCCEmotionType.Reproach;
			}

			return new OCCBaseEmotion(emoType, Math.Abs(praiseworthiness), evt.Id, direction, evt.EventName);
		}

		private static OCCBaseEmotion OCCAppraiseAttribution(IBaseEvent evt, float like)
		{
			const float magicFactor = 0.7f;
			OCCEmotionType emoType = (like >= 0)?OCCEmotionType.Love:OCCEmotionType.Hate;
			return new OCCBaseEmotion(emoType, Math.Abs(like)*magicFactor ,evt.Id, evt.Subject==null?Name.UNIVERSAL_SYMBOL:evt.Subject, evt.EventName);
		}

	


        private static OCCBaseEmotion AppraiseGoalSuccessProbability(IBaseEvent evt, float goalProbability, float previousProbabilityValue, float significance) {

            
			//Significante is too low
			var potential = significance;
		
            if(previousProbabilityValue == goalProbability)
                return new OCCBaseEmotion(OCCEmotionType.Hope,0, evt.Id, evt.EventName);


            if(goalProbability > previousProbabilityValue)
            {

                if(goalProbability == 1){
                    if(previousProbabilityValue <= 0.5)
                      return new OCCBaseEmotion(OCCEmotionType.Relief, Math.Abs(goalProbability) * potential, evt.Id, evt.EventName);
                    
                    else return new OCCBaseEmotion(OCCEmotionType.Satisfaction, Math.Abs(goalProbability) * potential, evt.Id, evt.EventName); 
                    }
                    else return new OCCBaseEmotion(OCCEmotionType.Hope, Math.Abs(goalProbability) * potential, evt.Id, evt.EventName);
            }

            else  //if(goalProbability < goal.Likelihood)
            {
                 if(goalProbability == 0)
                    if(previousProbabilityValue >= 0.5)
                        return new OCCBaseEmotion(OCCEmotionType.Disappointment, (1 - goalProbability) * potential, evt.Id, evt.EventName);
                 
                 else   return new OCCBaseEmotion(OCCEmotionType.FearsConfirmed, (1 - goalProbability) * potential, evt.Id, evt.EventName);

                    else return new OCCBaseEmotion(OCCEmotionType.Fear, (1 - goalProbability) * potential , evt.Id, evt.EventName);
            }
        }
    
		/*	if(hopeEmotion != null) {
				if(fearEmotion != null && fearEmotion.Intensity > hopeEmotion.Intensity) {
					potential = fearEmotion.Potential;
					finalEmotion = fearfullOutcome;
				}
				else {
					potential = hopeEmotion.Potential;
					finalEmotion = hopefullOutcome;
				}
			}
			else if(fearEmotion != null) {
				potential = fearEmotion.Potential;
				finalEmotion = fearfullOutcome;
			}
		
			//The goal importance now affects 66% of the final potential value for the emotion
			potential = (potential +  2* goalImportance) / 3;
		
			return new OCCBaseEmotion(finalEmotion, potential, evtId, eventName);
		}*/


        /*
         * 	private static OCCBaseEmotion AppraiseGoalEnd(OCCEmotionType hopefullOutcome, OCCEmotionType fearfullOutcome, IActiveEmotion hopeEmotion, IActiveEmotion fearEmotion, float goalImportance, uint evtId, Name eventName) {

			OCCEmotionType finalEmotion;
			float potential = goalImportance;
			finalEmotion = hopefullOutcome;
		
			if(hopeEmotion != null) {
				if(fearEmotion != null && fearEmotion.Intensity > hopeEmotion.Intensity) {
					potential = fearEmotion.Potential;
					finalEmotion = fearfullOutcome;
				}
				else {
					potential = hopeEmotion.Potential;
					finalEmotion = hopefullOutcome;
				}
			}
			else if(fearEmotion != null) {
				potential = fearEmotion.Potential;
				finalEmotion = fearfullOutcome;
			}
		
			//The goal importance now affects 66% of the final potential value for the emotion
			potential = (potential +  2* goalImportance) / 3;
		
			return new OCCBaseEmotion(finalEmotion, potential, evtId, eventName);
		}
         * 
         * */
		///// <summary>
		///// Appraises a Goal's success according to the emotions that the agent is experiencing
		///// </summary>
		///// <param name="hopeEmotion">the emotion of Hope for achieving the goal that the character feels</param>
		///// <param name="fearEmotion">the emotion of Fear for not achieving the goal that the character feels</param>
		///// <param name="goalImportance">how important is the goal to the agent</param>
		///// <param name="evt">The event that triggered the emotion</param>
		///// <returns>the emotion created</returns>
		//private static OCCBaseEmotion AppraiseGoalSuccess(IActiveEmotion hopeEmotion, IActiveEmotion fearEmotion, float goalImportance, uint evt) {
		//	return AppraiseGoalEnd(OCCEmotionType.Satisfaction,OCCEmotionType.Relief,hopeEmotion,fearEmotion,goalImportance,evt);
		//}

		///// <summary>
		///// Appraises a Goal's Failure according to the emotions that the agent is experiencing
		///// </summary>
		///// <param name="hopeEmotion">the emotion of Hope for achieving the goal that the character feels</param>
		///// <param name="fearEmotion">the emotion of Fear for not achieving the goal that the character feels</param>
		///// <param name="goalImportance">how important is the goal to the agent</param>
		///// <param name="evt">The event that triggered the emotion</param>
		///// <returns></returns>
		//public static OCCBaseEmotion AppraiseGoalFailure(IActiveEmotion hopeEmotion, IActiveEmotion fearEmotion, float goalImportance, uint evt) {
		//	return AppraiseGoalEnd(OCCEmotionType.Disappointment,OCCEmotionType.FearsConfirmed,hopeEmotion,fearEmotion,goalImportance,evt);
		//}

		///// <summary>
		///// Appraises a Goal's likelihood of succeeding
		///// </summary>
		///// <param name="e">The event that triggered the emotion</param>
		///// <param name="goalConduciveness">???????</param>
		///// <param name="prob">probability of sucess</param>
		///// <returns></returns>
	//	public static OCCBaseEmotion AppraiseGoalSuccessProbability(uint evt, float goalConduciveness, float prob) {
	//		return new OCCBaseEmotion(OCCEmotionType.Hope, prob * goalConduciveness, evt);
	//	}

		///// <summary>
		///// Appraises a Goal's likelihood of failure
		///// </summary>
		///// <param name="e">The event that triggered the emotion</param>
		///// <param name="goalConduciveness">???????</param>
		///// <param name="prob">probability of failure</param>
		///// <returns></returns>
		//public static OCCBaseEmotion AppraiseGoalFailureProbability(uint evt, float goalConduciveness, float prob)
		//{
		//	return new OCCBaseEmotion(OCCEmotionType.Fear, prob * goalConduciveness, evt);
		//}

		public IEnumerable<IEmotion> AffectDerivation(EmotionalAppraisalAsset emotionalModule, IAppraisalFrame frame)
		{
			var evt = frame.AppraisedEvent;
            

			if(frame.ContainsAppraisalVariable(OCCAppraisalVariables.DESIRABILITY) && frame.ContainsAppraisalVariable(OCCAppraisalVariables.PRAISEWORTHINESS))
			{
				float desirability = frame.GetAppraisalVariable(OCCAppraisalVariables.DESIRABILITY);
				float praiseworthiness = frame.GetAppraisalVariable(OCCAppraisalVariables.PRAISEWORTHINESS);

				var composedEmotion = OCCAppraiseCompoundEmotions(evt, desirability, praiseworthiness);
				if (composedEmotion != null)
					yield return composedEmotion;
			}
			
			if(frame.ContainsAppraisalVariable(OCCAppraisalVariables.DESIRABILITY))
			{
				float desirability = frame.GetAppraisalVariable(OCCAppraisalVariables.DESIRABILITY);
				if(desirability!=0)
				{
					// yield return OCCAppraiseWellBeing(evt.Id, evt.EventName, desirability * 0.5f);
                    int counter = 0;
					foreach(string variable in frame.AppraisalVariables.Where(v => v.StartsWith(OCCAppraisalVariables.DESIRABILITY_FOR_OTHER)))
					{
                        counter++;
						string other = variable.Substring(OCCAppraisalVariables.DESIRABILITY_FOR_OTHER.Length);
						float desirabilityForOther = frame.GetAppraisalVariable(variable);
						if (desirabilityForOther != 0)
							yield return OCCAppraiseFortuneOfOthers(evt, desirability, desirabilityForOther, other);
                       
					}
                    if(counter == 0)
                         yield return OCCAppraiseWellBeing(evt.Id, evt.EventName, desirability);
				}
			}
            


            foreach(string variable in frame.AppraisalVariables.Where(v => v.StartsWith(OCCAppraisalVariables.PRAISEWORTHINESS)))
					{
				        float praiseworthiness = frame.GetAppraisalVariable(variable);
						string other = variable.Substring(OCCAppraisalVariables.PRAISEWORTHINESS.Length);
						if (other == null || other == " ")
							yield return OCCAppraisePraiseworthiness(evt, praiseworthiness, "SELF");

                        else yield return OCCAppraisePraiseworthiness(evt, praiseworthiness, other);
                       
					}


			/*if(frame.ContainsAppraisalVariable(OCCAppraisalVariables.PRAISEWORTHINESS))
			{
				float praiseworthiness = frame.GetAppraisalVariable(OCCAppraisalVariables.PRAISEWORTHINESS);
				if (praiseworthiness != 0)
					yield return OCCAppraisePraiseworthiness(evt, praiseworthiness, frame.Perspective);
			}*/





			if(frame.ContainsAppraisalVariable(OCCAppraisalVariables.LIKE))
			{
				float like = frame.GetAppraisalVariable(OCCAppraisalVariables.LIKE);
				if (like != 0)
					yield return OCCAppraiseAttribution(evt, like);
			}
			
		   foreach(string variable in frame.AppraisalVariables.Where(v => v.StartsWith(OCCAppraisalVariables.GOALSUCCESSPROBABILITY)))
			{
				float goalSuccessProbability = frame.GetAppraisalVariable(variable);
			    
                string goalName = variable.Substring(OCCAppraisalVariables.GOALSUCCESSPROBABILITY.Length + 1);
                    
                 var goals = emotionalModule.GetAllGoals().ToList();


                GoalDTO g =goals.Find(x=> goalName.ToString() == x.Name.ToString());
               
                if(g == null) continue;

                var previousValue = g.Likelihood;
               
                g.Likelihood = goalSuccessProbability;
				
             
                yield return AppraiseGoalSuccessProbability(evt, goalSuccessProbability, previousValue, g.Significance);
					}
            }
        
    

                /*
					var status = frame.GetAppraisalVariable(OCCAppraisalVariables.GOALSTATUS);
					if(status == GOALUNCONFIRMED)
					{
						float prob = frame.GetAppraisalVariable(OCCAppraisalVariables.SUCCESSPROBABILITY);
						if (prob != 0)
							yield return AppraiseGoalSuccessProbability(evt.Id, prob);
					
						prob = frame.GetAppraisalVariable(OCCAppraisalVariables.FAILUREPROBABILITY);
						if (prob != 0)
							yield return AppraiseGoalFailureProbability(evt.Id, goalConduciveness, prob);
					}
			//		else 
			//		{
			//			//TODO find a better way to retrive the active emotions, without allocating extra KB
			//			var fear = emotionalModule.EmotionalState.GetEmotion(new OCCBaseEmotion(OCCEmotionType.Fear, 0, evt.Id));
			//			var hope = emotionalModule.EmotionalState.GetEmotion(new OCCBaseEmotion(OCCEmotionType.Hope, 0, evt.Id));

			//			if (status == GOALCONFIRMED)
			//				yield return AppraiseGoalSuccess(hope, fear, goalConduciveness, evt.Id);
			//			else if (status == GOALDISCONFIRMED)
			//				yield return AppraiseGoalFailure(hope, fear, goalConduciveness, evt.Id);
			//		}
			//	}
			//}
		}*/

		public short AffectDerivationWeight
		{
			get { return 1; }
		}

		public void InverseAffectDerivation(EmotionalAppraisalAsset emotionalModule, IEmotion emotion, IWritableAppraisalFrame frame)
		{
		 const float MAGIC_VALUE_FOR_LOVE = 1.43f;
			//TODO improve this code

			//ignoring mood for now

			EmotionDispositionDTO emotionDisposition = emotionalModule.EmotionDispositions.FirstOrDefault(e => e.Emotion == emotion.EmotionType);
		    if (emotionDisposition == null)
		    {
		        emotionDisposition = emotionalModule.DefaultEmotionDisposition;
		    }

			int threshold = emotionDisposition.Threshold;
			float potentialValue = emotion.Potential + threshold;

			if(emotion.EmotionType == OCCEmotionType.Love.Name)
			{
				frame.SetAppraisalVariable(OCCAppraisalVariables.LIKE, potentialValue * MAGIC_VALUE_FOR_LOVE);
			}
			else if(emotion.EmotionType == OCCEmotionType.Hate.Name)
			{
				frame.SetAppraisalVariable(OCCAppraisalVariables.LIKE, potentialValue * -MAGIC_VALUE_FOR_LOVE);
			}
			else
			if (emotion.EmotionType == OCCEmotionType.Joy.Name)
			{
				frame.SetAppraisalVariable(OCCAppraisalVariables.DESIRABILITY, potentialValue);
			}
			else if (emotion.EmotionType == OCCEmotionType.Distress.Name)
			{
				frame.SetAppraisalVariable(OCCAppraisalVariables.DESIRABILITY, -potentialValue);
			}
			else if (emotion.EmotionType == OCCEmotionType.Pride.Name)
			{
				frame.SetAppraisalVariable(OCCAppraisalVariables.PRAISEWORTHINESS, potentialValue);
			}
			else if (emotion.EmotionType == OCCEmotionType.Shame.Name)
			{
				frame.SetAppraisalVariable(OCCAppraisalVariables.PRAISEWORTHINESS, -potentialValue);
			}
			else if (emotion.EmotionType == OCCEmotionType.Gloating.Name)
			{
				frame.SetAppraisalVariable(OCCAppraisalVariables.DESIRABILITY, potentialValue);
				frame.SetAppraisalVariable(OCCAppraisalVariables.DESIRABILITY_FOR_OTHER, -potentialValue);
			}
			else if (emotion.EmotionType == OCCEmotionType.HappyFor.Name)
			{
				frame.SetAppraisalVariable(OCCAppraisalVariables.DESIRABILITY, potentialValue);
				frame.SetAppraisalVariable(OCCAppraisalVariables.DESIRABILITY_FOR_OTHER, potentialValue);
			}
			else if (emotion.EmotionType == OCCEmotionType.Pitty.Name)
			{
				frame.SetAppraisalVariable(OCCAppraisalVariables.DESIRABILITY, -potentialValue);
				frame.SetAppraisalVariable(OCCAppraisalVariables.DESIRABILITY_FOR_OTHER, -potentialValue);
			}
			else if (emotion.EmotionType == OCCEmotionType.Resentment.Name)
			{
				frame.SetAppraisalVariable(OCCAppraisalVariables.DESIRABILITY_FOR_OTHER, potentialValue);
			}
			else if (emotion.EmotionType == OCCEmotionType.Gratification.Name)
			{
				frame.SetAppraisalVariable(OCCAppraisalVariables.DESIRABILITY, potentialValue);
				frame.SetAppraisalVariable(OCCAppraisalVariables.DESIRABILITY_FOR_OTHER, potentialValue);
			}
			else if (emotion.EmotionType == OCCEmotionType.Anger.Name)
			{
				frame.SetAppraisalVariable(OCCAppraisalVariables.DESIRABILITY, -potentialValue);
				frame.SetAppraisalVariable(OCCAppraisalVariables.DESIRABILITY_FOR_OTHER, -potentialValue);
			}
		}
	}
}