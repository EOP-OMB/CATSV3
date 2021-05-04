using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace Mod.CatsV3.Email
{
    public enum CATSNotificationTypeEnum
    {
        [Description("EmailPackageLetterCreated")]
        LetterCreated = 1,
        [Description("EmailPackageRejected")]
        LetterRejected = 2,
        [Description("EmailPackageCopied")]
        LetterCopied = 3,
        [Description("EmailPackageLaunchReview")]
        LaunchReview = 4,
        [Description("EmailPackageAddOriginators")]
        AddOriginators = 5,
        [Description("EmailPackageAddReviewers")]
        AddReviewers = 6,
        [Description("EmailPackageAddFYIUsers")]
        AddFYIUsers = 7,
        [Description("EmailPackageReviewCompleted")]
        ReviewCompleted = 8,
        [Description("EmailPackageReviewDraft")]
        ReviewDraft = 9,
        [Description("EmailPackageClose")]
        PackageClosed = 10,
        [Description("EmailPackageReopen")]
        PackageReopen = 11,
        [Description("EmailPackageCloseAdminConsole")]
        CloseAdminConsole = 12,
        [Description("EmailPackageToSupport")]
        ToSupport = 13,
        [Description("EmailPackageReviewReminder")]
        ReviewReminder = 14

        //CATSNotificationTypeEnum myLocal = CATSNotificationTypeEnum.LetterCreated;
        //print(myLocal.ToDescriptionString());
    }
}
