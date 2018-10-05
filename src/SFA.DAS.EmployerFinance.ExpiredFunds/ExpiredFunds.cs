﻿using System;
using System.Collections.Generic;
using System.Linq;
using SFA.DAS.EmployerFinance.Domain.ExpiredFunds;

namespace SFA.DAS.EmployerFinance.ExpiredFunds
{
    public class ExpiredFunds : IExpiredFunds
    {
        public decimal GetExpiringFundsByDate(Dictionary<CalendarPeriod, decimal> fundsIn, Dictionary<CalendarPeriod, decimal> fundsOut, DateTime date)
        {
          return GetExpiringFundsByDate(fundsIn, fundsOut,date, null, 24);
        }

        public decimal GetExpiringFundsByDate(Dictionary<CalendarPeriod, decimal> fundsIn, Dictionary<CalendarPeriod, decimal> fundsOut, DateTime date, Dictionary<CalendarPeriod, decimal> expired, int expiryPeriod)
        {
            var expiredFunds = GetExpiringFunds(fundsIn, fundsOut, expired, expiryPeriod);

            if (expiredFunds.Any())
            {
                var expiredFundsKey = expiredFunds.Keys.SingleOrDefault(key => key.Year.Equals(date.Year) && key.Month.Equals(date.Month));
                if (expiredFundsKey != null)
                {
                    return expiredFunds[expiredFundsKey];
                }
            }

            return 0;
        }

        public Dictionary<CalendarPeriod, decimal> GetExpiringFunds(Dictionary<CalendarPeriod, decimal> fundsIn, Dictionary<CalendarPeriod, decimal> fundsOut)
        {
            return GetExpiringFunds(fundsIn, fundsOut, null, 24);
        }

        public Dictionary<CalendarPeriod, decimal> GetExpiringFunds(Dictionary<CalendarPeriod, decimal> fundsIn, Dictionary<CalendarPeriod, decimal> fundsOut, Dictionary<CalendarPeriod, decimal> expired, int expiryPeriod)
        {
            if (fundsIn == null)
            {
                throw new ArgumentNullException(nameof(fundsIn));
            }

            if (fundsOut == null)
            {
                throw new ArgumentNullException(nameof(fundsOut));
            }
            
            CalculateAndApplyExpiredFundsToFundsOut(fundsOut, expired);

            //CalculateAndApplyRefundsToFundsOut(fundsOut);

            CalculatedAndApplyRefundsToFundsIn(fundsIn, fundsOut);

            CalculateAndApplyAdjustmentsToFundsIn(fundsIn, expiryPeriod);
            
            var expiredFunds = new Dictionary<CalendarPeriod, decimal>();

            foreach (var fundsInPair in fundsIn.OrderBy(c => c.Key))
            {
                var expiryDateOfFundsIn = new DateTime(fundsInPair.Key.Year, fundsInPair.Key.Month, 1).AddMonths(expiryPeriod);
                var amountDueToExpire = fundsInPair.Value;


                var alreadyExpiredAmount = expired?.Keys.FirstOrDefault(c => c.Year.Equals(expiryDateOfFundsIn.Year)
                                                                         && c.Month.Equals(expiryDateOfFundsIn.Month));

                if (alreadyExpiredAmount != null)
                {
                    amountDueToExpire = expired[alreadyExpiredAmount];
                }
                else
                {
                    amountDueToExpire = amountDueToExpire > 0
                        ? CalculateExpiryAmount(fundsOut, expiryDateOfFundsIn, amountDueToExpire)
                        : 0;
                }
                

                expiredFunds.Add(new CalendarPeriod(expiryDateOfFundsIn.Year, expiryDateOfFundsIn.Month), amountDueToExpire);
            }

            return expiredFunds;
        }

        private void CalculateAndApplyRefundsToFundsOut(Dictionary<CalendarPeriod, decimal> fundsOut)
        {
            if (fundsOut != null && fundsOut.Any(c => c.Value < 0))
            {
                foreach (var refund in fundsOut.Where(c => c.Value < 0)
                    .ToDictionary(key => key.Key, value => value.Value))
                {
                    var refundAmount = refund.Value * -1;
                    
                    foreach (var fundsOutToAdjust in fundsOut.Where(c => c.Value > 0)
                        .ToDictionary(key => key.Key, value => value.Value))
                    {
                        
                        if (fundsOutToAdjust.Value >= refundAmount)
                        {
                            fundsOut[fundsOutToAdjust.Key] = fundsOutToAdjust.Value - refundAmount;
                            break;
                        }
                        
                        refundAmount = refundAmount - fundsOutToAdjust.Value;
                        fundsOut[fundsOutToAdjust.Key] = 0;
                    }
                }
            }
        }

        private void CalculatedAndApplyRefundsToFundsIn(Dictionary<CalendarPeriod, decimal> fundsIn, Dictionary<CalendarPeriod, decimal> fundsOut)
        {
            var refunds = fundsOut.Where(c => c.Value < 0).ToDictionary(key => key.Key, value => value.Value);
            if (refunds.Any())
            {
                foreach (var refund in refunds)
                {
                    if (fundsIn.ContainsKey(refund.Key))
                    {
                        fundsIn[refund.Key] += refund.Value * -1;
                    }
                    else
                    {
                        fundsIn.Add(refund.Key,refund.Value*-1);
                    }
                }
            }
        }

        private void CalculateAndApplyExpiredFundsToFundsOut(Dictionary<CalendarPeriod, decimal> fundsOut, Dictionary<CalendarPeriod, decimal> expired)
        {
            if (expired!=null && expired.Any(c=>c.Value> 0))
            {
                foreach (var expiredAmount in expired)
                {
                    var amount = expiredAmount.Value;

                    var fundsOutAvailable = fundsOut
                        .Where(c => c.Value > 0 && c.Key < expiredAmount.Key)
                        .ToList();

                    foreach (var payment in fundsOutAvailable)
                    {
                        if (payment.Value >= amount)
                        {
                            fundsOut[payment.Key] = payment.Value - amount;
                            break;
                        }
                        amount = amount - payment.Value;
                        fundsOut[payment.Key] = 0;
                    }

                }   
            }
        }

        private void CalculateAndApplyAdjustmentsToFundsIn(Dictionary<CalendarPeriod, decimal> fundsIn, int expiryPeriod)
        {
            if (fundsIn.Any(c => c.Value < 0))
            {
                var adjustmentsIn = fundsIn.Where(c => c.Value < 0).ToDictionary(key => key.Key, value => value.Value);

                foreach (var adjustment in adjustmentsIn.OrderBy(c => c.Key))
                {
                    var adjustmentAmount = adjustment.Value * -1;

                    foreach (var fundsInValue in fundsIn.Where(c => c.Value > 0)
                                                        .ToDictionary(c => c.Key, c => c.Value)
                                                        .OrderBy(c => c.Key))
                    {
                        

                        if (FundsAreInExpiryPeriod(fundsInValue, adjustment.Key, expiryPeriod))
                        {
                            if (fundsInValue.Value >= adjustmentAmount)
                            {
                                fundsIn[fundsInValue.Key] = fundsInValue.Value - adjustmentAmount;
                                break;
                            }

                            if (fundsInValue.Value < adjustmentAmount)
                            {
                                fundsIn[fundsInValue.Key] = 0;
                                adjustmentAmount = adjustmentAmount - fundsInValue.Value;
                            }
                        }
                    }
                }
            }
        }

        private static decimal CalculateExpiryAmount(IDictionary<CalendarPeriod, decimal> fundsOut, DateTime expiryDate, decimal expiryAmount)
        {
            var fundsOutAvailable = fundsOut
                .Where(c => new DateTime(c.Key.Year, c.Key.Month, 1) < expiryDate && c.Value > 0)
                .ToList();

            if (!fundsOutAvailable.Any())
            {
                return expiryAmount;
            }
            
            foreach (var spentFunds in fundsOutAvailable)
            {
                if (spentFunds.Value >= expiryAmount)
                {
                    fundsOut[spentFunds.Key] = spentFunds.Value - expiryAmount;
                    expiryAmount = 0;
                    break;
                }

                expiryAmount = expiryAmount - spentFunds.Value;
                fundsOut[spentFunds.Key] = 0;
            }

            return expiryAmount;
        }

        private static bool FundsAreInExpiryPeriod(KeyValuePair<CalendarPeriod, decimal> fundsInValue, CalendarPeriod adjustment, int expiryPeriod)
        {
            var adjustmentStartPeriod = new DateTime(adjustment.Year, adjustment.Month, 1).AddMonths(expiryPeriod * -1);
            var adjustmentEndPeriod = new DateTime(adjustment.Year, adjustment.Month, 1);
            
            return new DateTime(fundsInValue.Key.Year, fundsInValue.Key.Month, 1) > adjustmentStartPeriod &&
                   new DateTime(fundsInValue.Key.Year, fundsInValue.Key.Month, 1) <= adjustmentEndPeriod;
        }
    }
}