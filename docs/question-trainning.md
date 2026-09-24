#7 The current invoice period is 01/10–31/12. There are 4 scheduled updates with effective dates: 01/09, 01/10, 15/11, and 15/01. How does the system handle each case?

Short answer: The workspace covers only part of this. What it does show is that the system handles a scheduled update in two separate ways. When you save it, the system compares the effective date with today's date (24/09/2026), not with the invoice period. Separately, it marks some updates as "pre-invoice" by comparing the effective date with the service's next invoice date. How the invoice run then splits or prorates the 01/10–31/12 period across these dates is not in the workspace.

I assumed the dates are 01/09/2026, 01/10/2026, 15/11/2026 and 15/01/2027.

What happens in each case
Effective date	On save (compared with today)	"Pre-invoice" flag (compared with next invoice date)	Effect on the 01/10–31/12 invoice
01/09 (before the period, in the past)	✅ Queued for immediate processing, because the date is today or earlier	✅ Flagged if the service was already invoiced and its next invoice date is after 01/09 (for example 01/10). If it was never invoiced, it is flagged only if 01/09 is after the invoice start date and before the next invoice date	⚠️ Not proven. The flag suggests a separate one-off correction for September, but that is a guess from its name
01/10 (first day of the period)	✅ Not processed yet. It stays pending because the date is in the future. A quantity change shows as a "future quantity update"	Next invoice date 01/10: ✅ not flagged (the date must be before the next invoice date). Next invoice date 01/01: ✅ flagged	⚠️ Not proven whether an invoice for this period made before 01/10 uses the new values
15/11 (mid-period)	✅ Not processed yet, stays pending	Period not yet invoiced (next invoice date 01/10): ✅ not flagged. Period already invoiced in advance (next invoice date 01/01): ✅ flagged as pre-invoice for an invoiced service	⚠️ Not proven whether the period is split at 15/11 (old price for 01/10–14/11, new from 15/11) or corrected afterwards
15/01 (next period)	✅ Not processed yet, stays pending	✅ Never flagged under either next invoice date, since it is after both	⚠️ Probably not included in this period's invoice, but the invoice run excluding it is not proven

....

#1 On 01/09, Customer A reported that they stopped working with us, so LostDate = 01/09. Today is 24/09, but IsCustomer is still true. Can the contact of Customer A still log in to CustomerWeb? If yes, when will they lose access?

#8. A bundle service has a parent quantity of 2. Component A: quantity = 4, unit price = 120/year, discount = 25%. Component B: quantity = 2, unit price = 50/month, Calculate selling price from components = true. What is the main price after recalculation? What happens if the bundle has already been invoiced before?

#9 Customer B sends a completely new email (not a reply) with the subject "Broken printer". This morning, there was already a WO for the same customer with the same subject. In the email body, the customer says "related to WO #10234", but WO #10234 belongs to another customer. Which WO should this email be added to, or should a new WO be created?

#10 When is a Worklog created? How is the Worklog number shown? Does the Worklog contain billable hours or worked hours? When is the Worklog sent by email, and to whom?

11. The Activity has an hourly price of 1,200 and VAT of 25%. 
The registrations during the month are:
-Normal (100%): 3 billable hours and 0.5 automation billable hours. The registration has an hourly rate of 1,000.
-Overtime (150%): 2 billable hours.
The invoice date is 20/09. How many lines are there, what are the unit price, quantity, and total amount for each line? What is the period of the lines?