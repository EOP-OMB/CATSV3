/***** Assuming the CATSv3-DEV is already created. IF NOT PLEASE CREATE IT FIRST , or RUN THE SCRIPT IN YOUR OWN DATABASE BY CHANGING THE "USE [CATSv3-DEV]"
 WITH YOUR "USE [your CATS database]"
---------------------------------------------------------------------------------
****/
USE [CATSv3-DEV]
GO
/****** Object:  UserDefinedFunction [dbo].[ufn_report_Get_Members]    Script Date: 4/30/2021 9:57:21 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE FUNCTION [dbo].[ufn_report_Get_Members](@CATSID varchar(100),  @ROUND  varchar(Max))  
  RETURNS @report_Members TABLE (catsid varchar(100), currentRound varchar(Max),  originators varchar(Max), reviewers varchar(Max), fyiUsers varchar(Max))
AS   
 
BEGIN  
	DECLARE @Originators varchar(max);
	DECLARE @Reviewers varchar(max);
	DECLARE @FyiUsers  varchar(max);

	set @Originators = '';
	set @Reviewers = '';
	set @FyiUsers = '';

	DECLARE @Originators1 varchar(100);
	DECLARE @Reviewers1 varchar(100);
	DECLARE @FyiUsers1  varchar(100);

	
	  
	--PRINT '-------- ORIGINATORS --------';    
  
	DECLARE emp_cursorOri CURSOR FOR     
	SELECT distinct rev.OriginatorName  
	from [dbo].[Originator] as Rev
			inner join [dbo].[Collaboration] as Col
			on Rev.CollaborationId = Col.id
			inner join [dbo].[Correspondence] as Cor
			on col.CorrespondenceId = cor.id
			where col.CATSID = @CATSID and rev.RoundName like '%' + @ROUND + '%'
			order by rev.OriginatorName
  
	OPEN emp_cursorOri    
  
	FETCH NEXT FROM emp_cursorOri INTO @Originators1         
  
	WHILE @@FETCH_STATUS = 0    
	BEGIN 

		if @Originators = ''
			BEGIN
				set @Originators = @Originators1;
			END
		else
			BEGIN
				set @Originators = @Originators + ';' + @Originators1;
			END
             
		FETCH NEXT FROM emp_cursorOri INTO @Originators1
   
	END     
	CLOSE emp_cursorOri;    
	DEALLOCATE emp_cursorOri;    
	  
	--PRINT '-------- REVIEWERS --------';    
  
	DECLARE emp_cursorRev CURSOR FOR     
	SELECT distinct rev.ReviewerName 
	from [dbo].[Reviewer] as Rev
			inner join [dbo].[Collaboration] as Col
			on Rev.CollaborationId = Col.id
			inner join [dbo].[Correspondence] as Cor
			on col.CorrespondenceId = cor.id
			where col.CATSID = @CATSID and rev.RoundName like '%' + @ROUND + '%'
			order by rev.ReviewerName
  
	OPEN emp_cursorRev    
  
	FETCH NEXT FROM emp_cursorRev INTO @Reviewers1         
  
	WHILE @@FETCH_STATUS = 0    
	BEGIN 
		
		if @Reviewers = ''
			BEGIN
				set @Reviewers = @Reviewers1;
			END
		else
			BEGIN
				set @Reviewers = @Reviewers + ';' + @Reviewers1;
			END
             
		FETCH NEXT FROM emp_cursorRev     
		INTO @Reviewers1
   
	END     
	CLOSE emp_cursorRev;    
	DEALLOCATE emp_cursorRev;    

	   
	  
	--PRINT '-------- FYI USERS --------';    
  
	DECLARE emp_cursorFyi CURSOR FOR     
	SELECT distinct rev.FYIUserName  
	from [dbo].[FYIUser] as Rev
			inner join [dbo].[Collaboration] as Col
			on Rev.CollaborationId = Col.id
			inner join [dbo].[Correspondence] as Cor
			on col.CorrespondenceId = cor.id
			where col.CATSID = @CATSID and rev.RoundName like '%' + @ROUND + '%'
			order by rev.FYIUserName
  
	OPEN emp_cursorFyi    
  
	FETCH NEXT FROM emp_cursorFyi INTO @FyiUsers1         
  
	WHILE @@FETCH_STATUS = 0    
	BEGIN 
		if @FyiUsers = ''
			BEGIN
				set @FyiUsers = @FyiUsers1;
			END
		else
			BEGIN
				set @FyiUsers = @FyiUsers + ';' + @FyiUsers1;
			END
             
		FETCH NEXT FROM emp_cursorFyi     
		INTO @FyiUsers1
   
	END     
	CLOSE emp_cursorFyi;    
	DEALLOCATE emp_cursorFyi;    
	
	INSERT INTO @report_Members
    select @catsid, @ROUND , @Originators, @Reviewers, @FyiUsers
RETURN 
END; 
--select * from [dbo].[ufn_report_Get_Members]('2020-CATS-6494','Red Building General Counsel')


GO
/****** Object:  UserDefinedFunction [dbo].[ufn_report_Get_SSRS_Parameter_Sources]    Script Date: 4/30/2021 9:57:21 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE FUNCTION [dbo].[ufn_report_Get_SSRS_Parameter_Sources](@letterStatus varchar(100), @startDate datetime2(7), @endDate datetime2(7), @correspondentName varchar(500), @adminsitrationId  varchar(10) = '' )  
  RETURNS @report_ssrs_parameters_sources TABLE (catsids varchar(Max), reviewRounds varchar(Max),  leadOffices varchar(Max), letterTypes varchar(Max))
AS   
 
BEGIN  
	DECLARE @catsids varchar(max);
	DECLARE @reviewRounds varchar(max);
	DECLARE @leadOffices  varchar(max);
	DECLARE @letterTypes  varchar(max);

	set @catsids = '';
	set @reviewRounds = '';
	set @leadOffices = '';
	set @letterTypes = '';

	DECLARE @catsids1 varchar(100);
	DECLARE @reviewRounds1 varchar(max);
	DECLARE @leadOffices1  varchar(max);
	DECLARE @letterTypes1  varchar(max);

	
	  
	--PRINT '-------- CATSIDs --------';    
  
	DECLARE emp_cursorCatsIds CURSOR FOR     
	SELECT distinct CATSID  
	from [dbo].[Correspondence]
	where  CONVERT(varchar(20), coalesce(AdministrationId,'')) LIKE '%' + @adminsitrationId + '%' and IsDeleted = 0 and LetterStatus like '%' + @letterStatus + '%' and coalesce(CorrespondentName, '') like '%' + trim(@correspondentName) + '%'  and  CreatedTime between @startDate and @endDate
	order by CATSID 
  
	OPEN emp_cursorCatsIds    
  
	FETCH NEXT FROM emp_cursorCatsIds INTO @catsids1         
  
	WHILE @@FETCH_STATUS = 0    
	BEGIN 

		if @catsids = ''
			BEGIN
				set @catsids = @catsids1;
			END
		else
			BEGIN
				set @catsids = @catsids + ';' + @catsids1;
			END
             
		FETCH NEXT FROM emp_cursorCatsIds INTO @catsids1
   
	END     
	CLOSE emp_cursorCatsIds;    
	DEALLOCATE emp_cursorCatsIds;    
	  
	--PRINT '-------- Lead Offices --------';    
  
	DECLARE emp_cursorLead CURSOR FOR     
	    
	SELECT distinct LeadOfficeName  
	from [dbo].[Correspondence]
	where IsDeleted = 0 and LetterStatus like '%' + @letterStatus + '%' and  CreatedTime between @startDate and @endDate
	order by LeadOfficeName 
  
	OPEN emp_cursorLead    
  
	FETCH NEXT FROM emp_cursorLead INTO @leadOffices1         
  
	WHILE @@FETCH_STATUS = 0    
	BEGIN 
		
		if @leadOffices = ''
			BEGIN
				set @leadOffices = @leadOffices1;
			END
		else
			BEGIN
				set @leadOffices = @leadOffices + ';' + @leadOffices1;
			END
             
		FETCH NEXT FROM emp_cursorLead     
		INTO @leadOffices1
   
	END     
	CLOSE emp_cursorLead;    
	DEALLOCATE emp_cursorLead;    	   
	  
	--PRINT '-------- Review Round --------';    
  
	DECLARE emp_cursorRound CURSOR FOR  
	    
	SELECT distinct CurrentReview  
	from [dbo].[Correspondence]
	where IsDeleted = 0 and LetterStatus like '%' + @letterStatus + '%' and  CreatedTime between @startDate and @endDate
	order by CurrentReview 
  
	OPEN emp_cursorRound    
  
	FETCH NEXT FROM emp_cursorRound INTO @reviewRounds1         
  
	WHILE @@FETCH_STATUS = 0    
	BEGIN 
		if @reviewRounds = ''
			BEGIN
				set @reviewRounds = @reviewRounds1;
			END
		else
			BEGIN
				set @reviewRounds = @reviewRounds + ';' + @reviewRounds1;
			END
             
		FETCH NEXT FROM emp_cursorRound     
		INTO @reviewRounds1
   
	END     
	CLOSE emp_cursorRound;    
	DEALLOCATE emp_cursorRound;      	   
	  
	--PRINT '-------- letter Types --------';    
  
	DECLARE emp_cursorLetterType CURSOR FOR 
	    
	SELECT distinct LetterTypeName  
	from [dbo].[Correspondence]
	where IsDeleted = 0 and LetterStatus like '%' + @letterStatus + '%' and  CreatedTime between @startDate and @endDate
	order by LetterTypeName 
  
	OPEN emp_cursorLetterType    
  
	FETCH NEXT FROM emp_cursorLetterType INTO @letterTypes1         
  
	WHILE @@FETCH_STATUS = 0    
	BEGIN 
		if @letterTypes = ''
			BEGIN
				set @letterTypes = @letterTypes1;
			END
		else
			BEGIN
				set @letterTypes = @letterTypes + ';' + @letterTypes1;
			END
             
		FETCH NEXT FROM emp_cursorLetterType     
		INTO @letterTypes1
   
	END     
	CLOSE emp_cursorLetterType;    
	DEALLOCATE emp_cursorLetterType;      
	
	INSERT INTO @report_ssrs_parameters_sources
    select @catsids , @reviewRounds, @leadOffices, @letterTypes
RETURN 
END; 
--select * from [dbo].[ufn_report_Get_SSRS_Parameter_Sources]('open','1/1/2018','1/1/2020')


GO
/****** Object:  UserDefinedFunction [dbo].[ufn_report_Open_Status_Details]    Script Date: 4/30/2021 9:57:21 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE FUNCTION [dbo].[ufn_report_Open_Status_Details](@inCATSID varchar(100))

    RETURNS @report_statistics TABLE (
										  catsid varchar(100), currentRound varchar(100), leadOffice varchar(100), 
										  documentType varchar(100), createdBy varchar(100), correspondentName varchar(100),
										  subject varchar(100), actionDate varchar(100),
										  roundStartDate varchar(100),roundEndDate  varchar(100),completedRounds varchar(100),
										  numdaysPastDue int, numdaysInRound int, numdaysInSystem int
									  )
WITH EXECUTE AS CALLER
AS 
BEGIN

declare @catsid varchar(100), @currentRound varchar(100), @leadOffice varchar(100),
@documentType varchar(100), @createdBy varchar(100), @correspondentName varchar(100),
@subject varchar(100), @actionDate varchar(100),
@roundStartDate varchar(100), @roundDate  varchar(100), @completedRounds varchar(100),
@numdaysPastDue int, @numdaysInRound int, @numdaysInSystem int;

select @catsid=[CATSID], @currentRound=[CurrentReview], @leadOffice=[LeadOfficeName], @documentType=[LetterTypeName], @createdBy=[CreatedBy], @correspondentName=[CorrespondentName], 
@subject=[LetterSubject], @actionDate='action date', @roundStartDate='roundStartDate',@roundDate='roundEndDate', @completedRounds='completedRounds', 
@numdaysPastDue=0,@numdaysInRound=0,@numdaysInSystem=0
from [dbo].[Correspondence] -- where IsDeleted = 0;

--select @numOfOpenItems = count(CATSID) from [dbo].[Correspondence] where LetterStatus like '%open%' and IsDeleted = 0;
--select @numOfOpenInCollaboration = count(CATSID) from [dbo].[Correspondence] where LetterStatus like '%open%' and IsUnderReview = 1 and IsDeleted = 0;
--select @avgDaysInCollaboration = AVG(DATEDIFF(day,  CreatedTime, ModifiedTime)) from [dbo].[Correspondence] where LetterStatus like '%closed%' and CurrentReview = 'Final packaging'  and IsDeleted = 0;
--select @avgNumOfRoundPerItem = AVG(num) from (
--													select  X.[CollaborationId], count([RoundName]) Num
--													from (
--														select distinct [CollaborationId], [RoundName] from [dbo].[Reviewer] where IsDeleted = 0
--													) as X
--													group by X.[CollaborationId]
--													--order by X.[CollaborationId] asc
--													) as A


INSERT INTO @report_statistics
    select @catsid , @currentRound, @leadOffice, @documentType, @createdBy, @correspondentName, @subject, @actionDate,
			@roundStartDate, @roundDate, @completedRounds, @numdaysPastDue, @numdaysInRound, @numdaysInSystem
RETURN 
END

--SELECT * FROM  [dbo].[ufn_report_Open_Status_Details]()
GO
/****** Object:  UserDefinedFunction [dbo].[ufn_report_Open_Status_Details_Action_Date]    Script Date: 4/30/2021 9:57:21 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE FUNCTION [dbo].[ufn_report_Open_Status_Details_Action_Date](@CATSID varchar(100),  @ROUND  varchar(Max))  
  RETURNS varchar(100)
AS   
 
BEGIN  
	DECLARE @Action_Date varchar(50)
	DECLARE @CollaborationId int;
	
	--DECLARE @CATSID varchar(Max)
	--SET @CATSID = '2018-CATS-5268'
	--DECLARE @ROUND  varchar(Max)
	--SET @ROUND = 'Comms and Leg Affairs'
	--select @ROUND;
	select @Action_Date = b.ActionDate from 
	(
		select a.[CATSID], a.RoundName, max(a.ActionDate) ActionDate  from 
		(
			select Cor.[CATSID],rev.ReviewerName, rev.RoundName, 
			(case 
				when rev.RoundCompletedDate is not null then rev.RoundCompletedDate  
				when rev.DraftDate is not null then rev.DraftDate else ''
			end ) as ActionDate 
			from [dbo].[Reviewer] as Rev
			inner join [dbo].[Collaboration] as Col
			on Rev.CollaborationId = Col.id
			inner join [dbo].[Correspondence] as Cor
			on col.CorrespondenceId = cor.id
		) A 
		group by a.[CATSID],  a.RoundName--, a.CreatedTime
		--having a.[CATSID] = '2018-CATS-1666'--desc
	) B				
	where b.CATSID = @CATSID and b.RoundName like '%' + @ROUND + '%';

	--select @Action_Date

--order by b.CATSID, b.CreatedTime
	SET @Action_Date = format(cast(@Action_Date as date), 'MM/dd/yyyy');--convert(varchar(10), @Action_Date, 103);
	
	IF CHARINDEX('01/01/1900', @Action_Date) > 0
		BEGIN
			SET @Action_Date = null
		END
		

	RETURN @Action_Date;
END; 

--select [dbo].[ufn_report_Open_Status_Details_Action_Date]('2018-CATS-1919','Comms and Leg Affairs')

GO
/****** Object:  UserDefinedFunction [dbo].[ufn_report_Open_Status_Details_By_Reviewer_Action_Date]    Script Date: 4/30/2021 9:57:21 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE FUNCTION [dbo].[ufn_report_Open_Status_Details_By_Reviewer_Action_Date](@CATSID varchar(100),  @ROUND  varchar(Max))  
  RETURNS varchar(100)
AS   
 
BEGIN  
	DECLARE @Action_Date varchar(50)
	DECLARE @CollaborationId int;
	
	--DECLARE @CATSID varchar(Max)
	--SET @CATSID = '2018-CATS-5268'
	--DECLARE @ROUND  varchar(Max)
	--SET @ROUND = 'Comms and Leg Affairs'
	--select @ROUND;
	select @Action_Date = b.ActionDate from 
	(
		select a.[CATSID], a.RoundName,a.ReviewerUPN, max(a.ActionDate) ActionDate  from 
		(
			select Cor.[CATSID],rev.ReviewerName, rev.ReviewerUPN, rev.RoundName, 
			(case 
				when rev.RoundCompletedDate is not null then rev.RoundCompletedDate  
				when rev.DraftDate is not null then rev.DraftDate else ''
			end ) as ActionDate 
			from [dbo].[Reviewer] as Rev
			inner join [dbo].[Collaboration] as Col
			on Rev.CollaborationId = Col.id
			inner join [dbo].[Correspondence] as Cor
			on col.CorrespondenceId = cor.id
		) A 
		group by a.[CATSID],  a.RoundName, a.ReviewerUPN--, a.CreatedTime
		--having a.[CATSID] = '2018-CATS-1666'--desc
	) B				
	where b.CATSID = @CATSID  and b.RoundName like '%' + @ROUND + '%';

	--select @Action_Date

--order by b.CATSID, b.CreatedTime
	SET @Action_Date = format(cast(@Action_Date as date), 'MM/dd/yyyy');--convert(varchar(10), @Action_Date, 103);
	
	IF CHARINDEX('01/01/1900', @Action_Date) > 0
		BEGIN
			SET @Action_Date = null
		END
		

	RETURN @Action_Date;
END; 

--select [dbo].[ufn_report_Open_Status_Details_Action_Date]('2018-CATS-1919','Comms and Leg Affairs')

GO
/****** Object:  UserDefinedFunction [dbo].[ufn_report_Open_Status_Details_By_Reviewer_DaysCount_By_Date]    Script Date: 4/30/2021 9:57:21 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE FUNCTION [dbo].[ufn_report_Open_Status_Details_By_Reviewer_DaysCount_By_Date](@CATSID varchar(100), @currentRound  varchar(Max)) 

    RETURNS @report_DaysCount_By_Date TABLE (catsid varchar(100), currentRound varchar(Max), reviewerName varchar(200), daysPastdue int, daysInRound int, DaysInSystem int)
WITH EXECUTE AS CALLER
AS 
BEGIN

declare @daysPastdue int, @daysInRound int, @DaysInSystem int, @startDate datetime2(7) ;
declare @reviewerName varchar(200);
declare @roundStartDate varchar(100), @roundEndDate  varchar(100), @createdDate datetime2(7), @revcreatedDate datetime2(7),  @currentDate datetime2(7), @roundCompletedDate  datetime2(7);
set @currentDate = GETDATE();
set @startDate = CONVERT(DATETIME, 0);

select 
	@catsid = a.[CATSID], 
	@currentRound = a.RoundName, 
	@reviewerName = a.ReviewerUPN,
	@revcreatedDate = min(a.RevCreated), 
	@createdDate = min(a.[CreatedTime]), 
	@roundStartDate = min(a.[RoundStartDate]), 
	@roundEndDate = max(a.[RoundEndDate]),
	@roundCompletedDate = max(a.RoundCompletedDate) 
from 
		(
			select 
				Cor.[CATSID], 
				rev.ReviewerName,
				rev.ReviewerUPN,
				rev.RoundName, 
				rev.[CreatedTime] AS RevCreated, 
				Cor.[CreatedTime], 
				rev.[RoundStartDate], 
				rev.[RoundEndDate], 
				(case when rev.RoundCompletedDate is null then CONVERT(DATETIME, 0) else rev.RoundCompletedDate end) RoundCompletedDate
			from [dbo].[Reviewer] as Rev
			inner join [dbo].[Collaboration] as Col
			on Rev.CollaborationId = Col.id
			inner join [dbo].[Correspondence] as Cor
			on col.CorrespondenceId = cor.id
		) as A 
		group by a.[CATSID],  a.RoundName, a.ReviewerUPN
		having a.[CATSID] LIKE '%' +  @CATSID + '%' --and a.RoundName like '%'-- +  @currentRound + '%'--desc


IF @roundCompletedDate > @startDate
	begin		
		select  @daysInRound = [dbo].[ufn_report_statistics_Calculate_numOfDays_Exclude_WkEnd](@revcreatedDate, @roundCompletedDate);
		select  @daysPastdue = [dbo].[ufn_report_statistics_Calculate_numOfDays_Exclude_WkEnd](@roundCompletedDate, @roundEndDate);
		select  @DaysInSystem = [dbo].[ufn_report_statistics_Calculate_numOfDays_Exclude_WkEnd](@createdDate,  @roundCompletedDate);
	end
else
	begin	
		select  @daysInRound = [dbo].[ufn_report_statistics_Calculate_numOfDays_Exclude_WkEnd](@revcreatedDate,  @currentDate);
		select  @daysPastdue = [dbo].[ufn_report_statistics_Calculate_numOfDays_Exclude_WkEnd](@currentDate, @roundEndDate);
		select  @DaysInSystem = [dbo].[ufn_report_statistics_Calculate_numOfDays_Exclude_WkEnd](@createdDate,  @currentDate);
	end

IF  @daysPastdue >= 0
	BEGIN
		SET @daysPastdue = 0;
	END
ELSE
	BEGIN
		SET @daysPastdue = ABS(@daysPastdue) ;
	END


INSERT INTO @report_DaysCount_By_Date
    select @catsid , @currentRound, @reviewerName, abs( @daysPastdue), abs(@daysInRound), abs(@DaysInSystem)
RETURN 
END

--SELECT * FROM  [dbo].[ufn_report_Open_Status_Details_DaysCount_By_Date]('2020-cats-6494','Red Building General Counsel')
--SELECT * FROM  [dbo].[ufn_report_Open_Status_Details_DaysCount_By_Date]('2020-cats-6484','PADs and Policy Officials')
--SELECT * FROM  [dbo].[ufn_report_Open_Status_Details_DaysCount_By_Date]('2020-cats-6486','Leg Affairs')
--SELECT * FROM  [dbo].[ufn_report_Open_Status_Details_DaysCount_By_Date]('2018-cats-5202','PADs and Policy Officials')
--SELECT * FROM  [dbo].[ufn_report_Open_Status_Details_DaysCount_By_Date]('2018-cats-5291','Red Building')

--daysPastdue FROM  [dbo].[ufn_report_Open_Status_Details_DaysCount_By_Date]('2020-cats-6494','Red Building General Counsel')

--daysInRound FROM  [dbo].[ufn_report_Open_Status_Details_DaysCount_By_Date]('2020-cats-6494','Red Building General Counsel')

--DaysInSystem FROM  [dbo].[ufn_report_Open_Status_Details_DaysCount_By_Date]('2020-cats-6494','Red Building General Counsel')
GO
/****** Object:  UserDefinedFunction [dbo].[ufn_report_Open_Status_Details_By_Reviewer_RoundStartEnd_Date]    Script Date: 4/30/2021 9:57:21 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE FUNCTION [dbo].[ufn_report_Open_Status_Details_By_Reviewer_RoundStartEnd_Date](@CATSID varchar(100),  @currentRound  varchar(Max)) 

    RETURNS @report_statistics TABLE (catsid varchar(100), currentRound varchar(Max),  roundStartDate varchar(100), roundEndDate  varchar(100) )
WITH EXECUTE AS CALLER
AS 
BEGIN

declare @roundStartDate varchar(100), @roundEndDate  varchar(100);

select @catsid = a.[CATSID], @currentRound = a.RoundName, @roundStartDate = min(a.[RoundStartDate]), @roundEndDate = max(a.[RoundEndDate]) from 
		(
			select Cor.[CATSID],rev.ReviewerName, rev.RoundName, rev.ReviewerUPN, rev.[RoundStartDate],rev.[RoundEndDate]
			from [dbo].[Reviewer] as Rev
			inner join [dbo].[Collaboration] as Col
			on Rev.CollaborationId = Col.id
			inner join [dbo].[Correspondence] as Cor
			on col.CorrespondenceId = cor.id
		) A 
		group by a.[CATSID],  a.RoundName, a.ReviewerUPN
		having a.[CATSID] = @CATSID and a.RoundName = @currentRound--desc


INSERT INTO @report_statistics
    select @catsid , @currentRound, format(cast(@roundStartDate as date), 'MM/dd/yyyy'), format(cast(@roundEndDate as date), 'MM/dd/yyyy') 
RETURN 
END

--SELECT roundstartdate FROM  [ufn_report_Open_Status_Details_RoundStartEnd_Date]('2020-cats-6492','PADs and Policy Officials')
--SELECT roundenddate FROM  [ufn_report_Open_Status_Details_RoundStartEnd_Date]('2020-cats-6492','PADs and Policy Officials')
GO
/****** Object:  UserDefinedFunction [dbo].[ufn_report_Open_Status_Details_DaysCount_By_Date]    Script Date: 4/30/2021 9:57:21 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE FUNCTION [dbo].[ufn_report_Open_Status_Details_DaysCount_By_Date](@CATSID varchar(100),  @currentRound  varchar(Max)) 

    RETURNS @report_DaysCount_By_Date TABLE (catsid varchar(100), currentRound varchar(Max),  daysPastdue int, daysInRound int, DaysInSystem int)
WITH EXECUTE AS CALLER
AS 
BEGIN

declare @daysPastdue int, @daysInRound int, @DaysInSystem int, @startDate datetime2(7) ;

declare @roundStartDate varchar(100), @roundEndDate  varchar(100), @itemStatus  varchar(100),
@LastUpdatedDate  datetime2(7), @createdDate datetime2(7), @revcreatedDate datetime2(7),  @currentDate datetime2(7), @roundCompletedDate  datetime2(7);
set @currentDate = GETDATE();
set @startDate = CONVERT(DATETIME, 0);

select 
	@catsid = a.[CATSID], 
	@currentRound = a.RoundName, 
	@itemStatus = a.LetterStatus,
	@revcreatedDate = min(a.RevCreated), 
	@createdDate = min(a.[CreatedTime]), 
	@LastUpdatedDate = max(a.ModifiedTime),
	@roundStartDate = min(a.[RoundStartDate]), 
	@roundEndDate = max(a.[RoundEndDate]),
	@roundCompletedDate = max(a.RoundCompletedDate) 
from 
		(
			select 
				Cor.[CATSID], 
				rev.ReviewerName, 
				rev.RoundName, 
				rev.[CreatedTime] AS RevCreated, 
				Cor.[CreatedTime], 
				Cor.[ModifiedTime], 
				Cor.[LetterStatus], 
				rev.[RoundStartDate], 
				rev.[RoundEndDate], 
				(case when rev.RoundCompletedDate is null then CONVERT(DATETIME, 0) else rev.RoundCompletedDate end) RoundCompletedDate
			from [dbo].[Reviewer] as Rev
			inner join [dbo].[Collaboration] as Col
			on Rev.CollaborationId = Col.id
			inner join [dbo].[Correspondence] as Cor
			on col.CorrespondenceId = cor.id
		) as A 
		group by a.[CATSID],  a.RoundName, a.LetterStatus--, a.CreatedTime
		having a.[CATSID] = @CATSID and a.RoundName = @currentRound--desc


--SET @createdDate = CONVERT(VARCHAR(15), @createdDate, 103);
--SET @revcreatedDate = CONVERT(VARCHAR(15), @revcreatedDate, 103);
--SET @currentDate = CONVERT(VARCHAR(15), @currentDate, 103);
--SET @roundCompletedDate = CONVERT(VARCHAR(15), @roundCompletedDate, 103);

IF @roundCompletedDate > @startDate
	begin		
		select  @daysInRound = [dbo].[ufn_report_statistics_Calculate_numOfDays_Exclude_WkEnd](@revcreatedDate, @roundCompletedDate);
		select  @daysPastdue = [dbo].[ufn_report_statistics_Calculate_numOfDays_Exclude_WkEnd](@roundCompletedDate, @roundEndDate);
		--select  @DaysInSystem = [dbo].[ufn_report_statistics_Calculate_numOfDays_Exclude_WkEnd](@createdDate,  @roundCompletedDate);
	end
else
	begin	
		select  @daysInRound = [dbo].[ufn_report_statistics_Calculate_numOfDays_Exclude_WkEnd](@revcreatedDate,  @currentDate);
		select  @daysPastdue = [dbo].[ufn_report_statistics_Calculate_numOfDays_Exclude_WkEnd](@currentDate, @roundEndDate);
		--select  @DaysInSystem = [dbo].[ufn_report_statistics_Calculate_numOfDays_Exclude_WkEnd](@createdDate,  @currentDate);
	end

iF @itemStatus = 'Closed'
	begin
		select  @DaysInSystem = [dbo].[ufn_report_statistics_Calculate_numOfDays_Exclude_WkEnd](@createdDate,  @LastUpdatedDate);
	end
else
	begin
		select  @DaysInSystem = [dbo].[ufn_report_statistics_Calculate_numOfDays_Exclude_WkEnd](@createdDate,  @currentDate);
	end

IF  @daysPastdue >= 0
	BEGIN
		SET @daysPastdue = 0;
	END
ELSE
	BEGIN
		SET @daysPastdue = ABS(@daysPastdue) ;
	END


INSERT INTO @report_DaysCount_By_Date
    select @catsid , @currentRound,abs( @daysPastdue), abs(@daysInRound), abs(@DaysInSystem)
RETURN 
END

--SELECT * FROM  [dbo].[ufn_report_Open_Status_Details_DaysCount_By_Date]('2020-cats-6494','Red Building General Counsel')
--SELECT * FROM  [dbo].[ufn_report_Open_Status_Details_DaysCount_By_Date]('2020-cats-6484','PADs and Policy Officials')
--SELECT * FROM  [dbo].[ufn_report_Open_Status_Details_DaysCount_By_Date]('2020-cats-6486','Leg Affairs')
--SELECT * FROM  [dbo].[ufn_report_Open_Status_Details_DaysCount_By_Date]('2018-cats-5202','PADs and Policy Officials')
--SELECT * FROM  [dbo].[ufn_report_Open_Status_Details_DaysCount_By_Date]('2018-cats-5291','Red Building')

--daysPastdue FROM  [dbo].[ufn_report_Open_Status_Details_DaysCount_By_Date]('2020-cats-6494','Red Building General Counsel')

--daysInRound FROM  [dbo].[ufn_report_Open_Status_Details_DaysCount_By_Date]('2020-cats-6494','Red Building General Counsel')

--DaysInSystem FROM  [dbo].[ufn_report_Open_Status_Details_DaysCount_By_Date]('2020-cats-6494','Red Building General Counsel')
GO
/****** Object:  UserDefinedFunction [dbo].[ufn_report_Open_Status_Details_RoundStartEnd_Date]    Script Date: 4/30/2021 9:57:21 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE FUNCTION [dbo].[ufn_report_Open_Status_Details_RoundStartEnd_Date](@CATSID varchar(100),  @currentRound  varchar(Max)) 

    RETURNS @report_statistics TABLE (catsid varchar(100), currentRound varchar(Max),  roundStartDate varchar(100), roundEndDate  varchar(100) )
WITH EXECUTE AS CALLER
AS 
BEGIN

declare @roundStartDate varchar(100), @roundEndDate  varchar(100);

select @catsid = a.[CATSID], @currentRound = a.RoundName, @roundStartDate = min(a.[RoundStartDate]), @roundEndDate = max(a.[RoundEndDate]) from 
		(
			select Cor.[CATSID],rev.ReviewerName, rev.RoundName, rev.[RoundStartDate],rev.[RoundEndDate]
			from [dbo].[Reviewer] as Rev
			inner join [dbo].[Collaboration] as Col
			on Rev.CollaborationId = Col.id
			inner join [dbo].[Correspondence] as Cor
			on col.CorrespondenceId = cor.id
		) A 
		group by a.[CATSID],  a.RoundName--, a.CreatedTime
		having a.[CATSID] = @CATSID and a.RoundName = @currentRound--desc


INSERT INTO @report_statistics
    select @catsid , @currentRound, format(cast(@roundStartDate as date), 'MM/dd/yyyy'), format(cast(@roundEndDate as date), 'MM/dd/yyyy') 
RETURN 
END

--SELECT roundstartdate FROM  [ufn_report_Open_Status_Details_RoundStartEnd_Date]('2020-cats-6492','PADs and Policy Officials')
--SELECT roundenddate FROM  [ufn_report_Open_Status_Details_RoundStartEnd_Date]('2020-cats-6492','PADs and Policy Officials')
GO
/****** Object:  UserDefinedFunction [dbo].[ufn_report_Open_Status_Details_RoundStartEnd_DateOld]    Script Date: 4/30/2021 9:57:21 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE FUNCTION [dbo].[ufn_report_Open_Status_Details_RoundStartEnd_DateOld](@CATSID varchar(100),  @currentRound  varchar(Max), @option varchar(100)) 

    RETURNS varchar (100)
WITH EXECUTE AS CALLER
AS 
BEGIN

declare @roundStartDate varchar(100), @roundEndDate  varchar(100), @RESULT varchar(100);

IF @option = 'start'
	BEGIN
		select @catsid = a.[CATSID], @currentRound = a.RoundName, @RESULT = min(a.[RoundStartDate]) 
		from 
		(
			select Cor.[CATSID],rev.ReviewerName, rev.RoundName, rev.[RoundStartDate]
			from [dbo].[Reviewer] as Rev
			inner join [dbo].[Collaboration] as Col
			on Rev.CollaborationId = Col.id
			inner join [dbo].[Correspondence] as Cor
			on col.CorrespondenceId = cor.id
		) A 
		group by a.[CATSID],  a.RoundName--, 
		having a.[CATSID] = @CATSID and a.RoundName = @currentRound--desc
	END
ELSE
	BEGIN
		select @catsid = a.[CATSID], @currentRound = a.RoundName, @RESULT = max(a.[RoundEndDate]) from 
		(
			select Cor.[CATSID],rev.ReviewerName, rev.RoundName,rev.[RoundEndDate]
			from [dbo].[Reviewer] as Rev
			inner join [dbo].[Collaboration] as Col
			on Rev.CollaborationId = Col.id
			inner join [dbo].[Correspondence] as Cor
			on col.CorrespondenceId = cor.id
		) A 
		group by a.[CATSID],  a.RoundName--, 
		having a.[CATSID] = @CATSID and a.RoundName = @currentRound--desc
	END



RETURN @RESULT 
END

--SELECT  [dbo].[ufn_report_Open_Status_Details_RoundStartEnd_Date]('2020-cats-6491','PADs and Policy Officials', 'start')
--SELECT [dbo].[ufn_report_Open_Status_Details_RoundStartEnd_Date]('2020-cats-6491','PADs and Policy Officials', 'end')
GO
/****** Object:  UserDefinedFunction [dbo].[ufn_report_statistics_Calculate_numOfDays_Exclude_WkEnd]    Script Date: 4/30/2021 9:57:21 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE FUNCTION [dbo].[ufn_report_statistics_Calculate_numOfDays_Exclude_WkEnd]( @startDate varchar(100), @endDate varchar(100))  
  RETURNS int
AS   
 
BEGIN  
	
	DECLARE @totaldays INT; 
	DECLARE @weekenddays INT;


	SET @totaldays = DATEDIFF(DAY, @startDate, @endDate) 
	SET @weekenddays = ((DATEDIFF(WEEK, @startDate, @endDate) * 2) + -- get the number of weekend days in between
						   CASE WHEN DATEPART(WEEKDAY, @startDate) = 1 THEN 1 ELSE 0 END + -- if selection was Sunday, won't add to weekends
						   CASE WHEN DATEPART(WEEKDAY, @endDate) = 6 THEN 1 ELSE 0 END)  -- if selection was Saturday, won't add to weekends

	--select @totaldays 
	--select @weekenddays;
	--select @totaldays - @weekenddays

	RETURN @totaldays - @weekenddays;
END; 

--select [dbo].[ufn_report_statistics_Calculate_numOfDays_Exclude_WkEnd]('2018-11-09 15:16:41.0000000', '2020-06-11 11:51:10.0000000')

GO
/****** Object:  UserDefinedFunction [dbo].[ufn_report_statistics_numberOfCATS_Open_items]    Script Date: 4/30/2021 9:57:21 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE FUNCTION [dbo].[ufn_report_statistics_numberOfCATS_Open_items](@startDate datetime2(7), @endDate datetime2(7), @correspondentName varchar(500), @adminsitrationId  varchar(10) = '')
    RETURNS @report_statistics TABLE (numOfCATSOpen int, numOfCATSOpenInCollaboration int, avgDaysInCollaboration int, avgNumOfRoundPerItem int)
WITH EXECUTE AS CALLER
AS 
BEGIN

DECLARE @numOfOpenItems int;
DECLARE @numOfOpenInCollaboration int; 
DECLARE @avgDaysInCollaboration int; 
DECLARE @avgNumOfRoundPerItem int;

select @numOfOpenItems = count(CATSID) from [dbo].[Correspondence] where IsDeleted != 1 AND  LetterStatus like '%open%' and  CONVERT(varchar(20), coalesce(AdministrationId,'')) LIKE '%' + @adminsitrationId + '%' and coalesce(CorrespondentName, '') like '%' + trim(@correspondentName) + '%' and CreatedTime between @startDate and @endDate;
		select @numOfOpenInCollaboration = count(CATSID) from [dbo].[Correspondence] where IsUnderReview = 1 and IsDeleted = 0 and  CONVERT(varchar(20), coalesce(AdministrationId,'')) LIKE '%' + @adminsitrationId + '%' AND   LetterStatus like '%open%' and coalesce(CorrespondentName, '') like '%' + trim(@correspondentName) + '%' and CreatedTime between @startDate and @endDate;
		select @avgDaysInCollaboration = AVG(DATEDIFF(day,  CreatedTime, ModifiedTime)) from [dbo].[Correspondence] where CONVERT(varchar(20), coalesce(AdministrationId,'')) LIKE '%' + @adminsitrationId + '%' AND   LetterStatus like '%closed%' and coalesce(CorrespondentName, '') like '%' + trim(@correspondentName) + '%' and CurrentReview = 'Final packaging'  and IsDeleted = 0 and CreatedTime between @startDate and @endDate;
		select @avgNumOfRoundPerItem = AVG(num) from (
															select  X.[CollaborationId], count([RoundName]) Num
															from (
																select distinct [CollaborationId], [RoundName] 
																from [dbo].[Reviewer] as R
																inner join Collaboration as C
																on C.id = R.CollaborationId
																inner join Correspondence as Corr
																on C.CorrespondenceId = corr.Id
																where R.IsDeleted = 0 and R.CreatedTime between @startDate and @endDate
																and coalesce(corr.CorrespondentName, '') like '%' + trim(@correspondentName) + '%'
																and  CONVERT(varchar(20), coalesce(corr.AdministrationId,'')) LIKE '%' + @adminsitrationId + '%' 
															) as X
															group by X.[CollaborationId]
															--order by X.[CollaborationId] asc
															) as A


INSERT INTO @report_statistics
    select  @numOfOpenItems, @numOfOpenInCollaboration, @avgDaysInCollaboration, @avgNumOfRoundPerItem
RETURN 
END

--SELECT * FROM  [dbo].[ufn_report_statistics_numberOfCATS_Open_items]('1/1/2018', '2/21/2021', '', '')
GO
/****** Object:  UserDefinedFunction [dbo].[ufnCheckExternalUsersFromList]    Script Date: 4/30/2021 9:57:21 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		Jean Pita Diomi kazadi
-- Create date: 07/31/2020
-- Description:	This Function if a value is present [dbo].[ExternalUser]. If not insert it
-- =============================================
Create FUNCTION [dbo].[ufnCheckExternalUsersFromList](@list varchar(Max) = NULL)  
RETURNS varchar(Max)   
AS   
-- Returns the stock level for the product.  
BEGIN  
	DECLARE @pos INT
	DECLARE @len INT
	DECLARE @countUsers INT
	DECLARE @value varchar(8000)
	DECLARE @res  varchar(8000)

	SET @list = @list + ';';
	set @res ='';

	set @pos = 0
	set @len = 0

	WHILE CHARINDEX(';', @list, @pos+1)>0
	BEGIN
		set @len = CHARINDEX(';', @list, @pos+1) - @pos
		set @value = SUBSTRING(@list, @pos, @len)

		select @countUsers =  COUNT([Title]) FROM [dbo].[ExternalUser]
		where [Title] = @value;

		IF @countUsers = 0
		BEGIN
			Set @countUsers = @countUsers;
			exec [dbo].[sp_InsertValueExternalUsers] @value
			--INSERT INTO [dbo].[ExternalUser] ([Title],[Name],[CreatedBy],[ModifiedBy])
			--VALUES (@value,@value,'','');
		END
		

		set @pos = CHARINDEX(';', @list, @pos+@len) +1
	END

	--PRINT @res 
       
    RETURN @res;
END; 

--select [dbo].[ufnCheckExternalUsersFromList]('test; doris')
GO
/****** Object:  UserDefinedFunction [dbo].[ufnGenerateAdministrationId]    Script Date: 4/30/2021 9:57:21 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE FUNCTION [dbo].[ufnGenerateAdministrationId]()
RETURNS int   
AS   
-- Returns the stock level for the product.  
BEGIN  
    DECLARE @ID INT, @CREATEDTIME DATETIME2(7), @INAUGURATIONDATE DATETIME2(7);

	SET @ID  = 0;
	SELECT @CREATEDTIME =  GETDATE();
	SELECT @ID = Id from  Administration WHERE IsCurrent = 1 ;
       
    RETURN @ID;
END; 

--select [dbo].[ufnGenerateCATSID]()
--SELECT SUBSTRING(catsid, 11, 10) from  [dbo].[Correspondence] WHERE [Id] = (SELECT MAX(Id) FROM [dbo].[Correspondence] where [FiscalYear] ='2020') ;

GO
/****** Object:  UserDefinedFunction [dbo].[ufnGenerateCATSID]    Script Date: 4/30/2021 9:57:21 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE FUNCTION [dbo].[ufnGenerateCATSID]()
RETURNS varchar(50)   
AS   
-- Returns the stock level for the product.  
BEGIN  
    DECLARE @ID INT, @YEAR VARCHAR(10), @CATSID VARCHAR(50), @LASTCATSID VARCHAR(50), @CREATEDTIME DATETIME2(7), @CREATEDBY VARCHAR(50);

	SET @ID  = 0;
	SELECT @YEAR =  DATEPART(year, CURRENT_TIMESTAMP);
	SELECT @LASTCATSID = SUBSTRING(catsid, 11, 10) from  [dbo].[Correspondence] WHERE CATSID = (SELECT MAX(CATSID) FROM [dbo].[Correspondence] where [FiscalYear] = @YEAR) ;
	SET @LASTCATSID = CONVERT(INT, CASE WHEN @LASTCATSID IS NULL OR @LASTCATSID = '' THEN '0' ELSE @LASTCATSID END) + 1;	
	SET @LASTCATSID = RIGHT('000'+ISNULL( @LASTCATSID,''),4); -- ALWAYS HAVE AT LEAST 5 DIGITS  WITH LEADING 0s IF SO
	SET @CATSID = @YEAR + '-CATS-' + @LASTCATSID  ;
    -- Insert statements for trigger here
       
    RETURN @CATSID;
END; 

--select [dbo].[ufnGenerateCATSID]()
--SELECT SUBSTRING(catsid, 11, 10) from  [dbo].[Correspondence] WHERE [Id] = (SELECT MAX(Id) FROM [dbo].[Correspondence] where [FiscalYear] ='2020') ;

GO
/****** Object:  Table [dbo].[Administration]    Script Date: 4/30/2021 9:57:21 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Administration](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Name] [varchar](50) NULL,
	[Description] [varchar](50) NULL,
	[IsCurrent] [bit] NULL,
	[CreatedBy] [varchar](50) NULL,
	[ModifiedBy] [varchar](50) NULL,
	[DeletedBy] [varchar](50) NULL,
	[CreatedTime] [datetime2](7) NULL,
	[ModifiedTime] [datetime2](7) NULL,
	[DeletedTime] [datetime2](7) NULL,
	[IsDeleted] [bit] NULL,
	[InaugurationDate] [datetime2](7) NULL,
 CONSTRAINT [PK_Administration] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Collaboration]    Script Date: 4/30/2021 9:57:21 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Collaboration](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[CorrespondenceId] [int] NOT NULL,
	[CATSID] [nchar](20) NULL,
	[CurrentRoundStartDate] [datetime2](7) NULL,
	[CurrentRoundEndDate] [datetime2](7) NULL,
	[CompletedRounds] [nvarchar](max) NULL,
	[BoilerPlate] [nchar](10) NULL,
	[CurrentOriginators] [nvarchar](max) NULL,
	[CurrentOriginatorsIds] [nvarchar](max) NULL,
	[CurrentReviewers] [nvarchar](max) NULL,
	[CurrentReviewersIds] [nvarchar](max) NULL,
	[CurrentFYIUsers] [nvarchar](max) NULL,
	[CurrentFYIUsersIds] [nvarchar](max) NULL,
	[CompletedReviewers] [nvarchar](max) NULL,
	[CompletedReviewersIds] [nvarchar](max) NULL,
	[DraftReviewers] [nvarchar](max) NULL,
	[DraftReviewersIds] [nvarchar](max) NULL,
	[CurrentActivity] [nvarchar](50) NULL,
	[SummaryMaterialBackground] [nvarchar](max) NULL,
	[ReviewInstructions] [nvarchar](max) NULL,
	[CreatedTime] [datetime2](7) NULL,
	[ModifiedTime] [datetime2](7) NULL,
	[DeletedTime] [datetime2](7) NULL,
	[CreatedBy] [nvarchar](50) NULL,
	[ModifiedBy] [nvarchar](50) NULL,
	[DeletedBy] [nvarchar](50) NULL,
	[IsDeleted] [bit] NULL,
	[SurrogateFullName] [nvarchar](500) NULL,
	[SurrogateUpn] [nvarchar](500) NULL,
 CONSTRAINT [PK_Collaboration] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[CompletedReviewRound]    Script Date: 4/30/2021 9:57:21 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[CompletedReviewRound](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[CorrespondenceId] [int] NULL,
	[CATSID] [nvarchar](500) NULL,
	[RoundName] [nvarchar](500) NULL,
	[CreatedBy] [nchar](10) NULL,
	[CreatedTime] [datetime2](7) NULL,
	[ModifiedBy] [nvarchar](500) NULL,
	[ModifiedTime] [datetime2](7) NULL,
	[DeletedBy] [nvarchar](500) NULL,
	[DeletedTime] [datetime2](7) NULL,
	[IsDeleted] [bit] NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Correspondence]    Script Date: 4/30/2021 9:57:21 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Correspondence](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[FolderId] [int] NULL,
	[AdministrationId] [int] NULL,
	[CATSID] [nvarchar](50) NULL,
	[LeadOfficeName] [nvarchar](500) NULL,
	[LeadOfficeUsersIds] [nvarchar](max) NULL,
	[LeadOfficeUsersDisplayNames] [nvarchar](max) NULL,
	[CopiedOfficeName] [nvarchar](max) NULL,
	[CopiedUsersIds] [nvarchar](max) NULL,
	[CopiedUsersDisplayNames] [nvarchar](max) NULL,
	[LetterTypeName] [nvarchar](500) NULL,
	[CorrespondentName] [nvarchar](max) NULL,
	[OtherSigners] [nvarchar](max) NULL,
	[LetterStatus] [nvarchar](50) NULL,
	[ReviewStatus] [nvarchar](50) NULL,
	[CurrentReview] [nvarchar](500) NULL,
	[LetterSubject] [nvarchar](max) NULL,
	[LetterCrossReference] [nvarchar](100) NULL,
	[WhFyi] [bit] NULL,
	[IsLetterRequired] [bit] NULL,
	[NotRequiredReason] [nvarchar](max) NULL,
	[ExternalAgencies] [nchar](20) NULL,
	[LetterDate] [datetime2](7) NULL,
	[LetterReceiptDate] [datetime2](7) NULL,
	[DueforSignatureByDate] [datetime2](7) NULL,
	[PADDueDate] [datetime2](7) NULL,
	[Rejected] [bit] NULL,
	[RejectionReason] [nvarchar](max) NULL,
	[RejectedLeadOffices] [nvarchar](max) NULL,
	[FiscalYear] [nvarchar](10) NULL,
	[IsPendingLeadOffice] [bit] NULL,
	[IsUnderReview] [bit] NULL,
	[Notes] [nvarchar](max) NULL,
	[ReasonsToReopen] [nvarchar](max) NULL,
	[IsReopen] [bit] NULL,
	[IsAdminReOpen] [bit] NULL,
	[AdminReOpenReason] [nvarchar](max) NULL,
	[AdminReOpenDate] [datetime2](7) NULL,
	[IsAdminClosure] [bit] NULL,
	[AdminClosureReason] [nvarchar](max) NULL,
	[AdminClosureDate] [datetime2](7) NULL,
	[CreatedDate] [datetime2](7) NULL,
	[ModifiedDate] [datetime2](7) NULL,
	[CreatedTime] [datetime2](7) NULL,
	[ModifiedTime] [datetime2](7) NULL,
	[DeletedTime] [datetime2](7) NULL,
	[CreatedBy] [nvarchar](500) NULL,
	[ModifiedBy] [nvarchar](500) NULL,
	[DeletedBy] [nvarchar](500) NULL,
	[IsDeleted] [bit] NULL,
	[Deleted] [bit] NULL,
	[IsSaved] [bit] NULL,
	[SurrogateFullName] [nvarchar](500) NULL,
	[SurrogateUpn] [nvarchar](500) NULL,
	[IsArchived] [bit] NULL,
	[IsEmailElligible] [bit] NULL,
 CONSTRAINT [PK_Correspondence] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Correspondence-Audit]    Script Date: 4/30/2021 9:57:21 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Correspondence-Audit](
	[Id] [int] NULL,
	[FolderId] [int] NULL,
	[CATSID] [nvarchar](50) NULL,
	[LeadOfficeName] [nvarchar](50) NULL,
	[LeadOfficeUsersIds] [nvarchar](max) NULL,
	[LeadOfficeUsersDisplayNames] [nvarchar](max) NULL,
	[CopiedOfficeName] [nvarchar](max) NULL,
	[CopiedUsersIds] [nvarchar](max) NULL,
	[CopiedUsersDisplayNames] [nvarchar](max) NULL,
	[LetterTypeName] [nvarchar](50) NULL,
	[CorrespondentName] [nvarchar](max) NULL,
	[OtherSigners] [nvarchar](max) NULL,
	[LetterStatus] [nvarchar](50) NULL,
	[ReviewStatus] [nvarchar](50) NULL,
	[CurrentReview] [nvarchar](max) NULL,
	[LetterSubject] [nvarchar](max) NULL,
	[LetterCrossReference] [nvarchar](100) NULL,
	[WhFyi] [bit] NULL,
	[IsLetterRequired] [bit] NULL,
	[NotRequiredReason] [nvarchar](50) NULL,
	[ExternalAgencies] [nchar](20) NULL,
	[LetterDate] [datetime2](7) NULL,
	[LetterReceiptDate] [datetime2](7) NULL,
	[DueforSignatureByDate] [datetime2](7) NULL,
	[PADDueDate] [datetime2](7) NULL,
	[Rejected] [bit] NULL,
	[RejectionReason] [nvarchar](max) NULL,
	[RejectedLeadOffices] [nvarchar](200) NULL,
	[FiscalYear] [nvarchar](10) NULL,
	[IsPendingLeadOffice] [bit] NULL,
	[IsUnderReview] [bit] NULL,
	[Notes] [nvarchar](max) NULL,
	[ReasonsToReopen] [nvarchar](max) NULL,
	[IsReopen] [bit] NULL,
	[IsAdminReOpen] [bit] NULL,
	[AdminReOpenReason] [nvarchar](max) NULL,
	[AdminReOpenDate] [datetime2](7) NULL,
	[IsAdminClosure] [bit] NULL,
	[AdminClosureReason] [nvarchar](max) NULL,
	[AdminClosureDate] [datetime2](7) NULL,
	[CreatedDate] [datetime2](7) NULL,
	[ModifiedDate] [datetime2](7) NULL,
	[CreatedTime] [datetime2](7) NULL,
	[ModifiedTime] [datetime2](7) NULL,
	[DeletedTime] [datetime2](7) NULL,
	[CreatedBy] [nvarchar](500) NULL,
	[ModifiedBy] [nvarchar](500) NULL,
	[DeletedBy] [nvarchar](500) NULL,
	[IsDeleted] [bit] NULL,
	[Deleted] [bit] NULL,
	[IsSaved] [bit] NULL,
	[SurrogateFullName] [nvarchar](500) NULL,
	[SurrogateUpn] [nvarchar](500) NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[CorrespondenceCopiedArchived]    Script Date: 4/30/2021 9:57:21 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[CorrespondenceCopiedArchived](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[CorrespondenceId] [int] NULL,
	[ArchivedUserUpn] [nvarchar](500) NULL,
	[ArchivedUserFullName] [nvarchar](500) NULL,
	[CreatedTime] [datetime2](7) NULL,
	[ModifiedTime] [datetime2](7) NULL,
	[DeletedTime] [datetime2](7) NULL,
	[CreatedBy] [nvarchar](500) NULL,
	[ModifiedBy] [nvarchar](500) NULL,
	[DeletedBy] [nvarchar](500) NULL,
	[IsDeleted] [bit] NULL,
 CONSTRAINT [PK_CorrespondenceCopiedArchived] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[CorrespondenceCopiedOffice]    Script Date: 4/30/2021 9:57:21 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[CorrespondenceCopiedOffice](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[CorrespondenceId] [int] NULL,
	[OfficeId] [int] NULL,
	[OfficeName] [nvarchar](100) NULL,
	[CATSID] [nvarchar](100) NULL,
	[CopiedUserUpn] [nvarchar](100) NULL,
	[CopiedUserFullName] [nvarchar](100) NULL,
	[CreatedTime] [datetime2](7) NULL,
	[ModifiedTime] [datetime2](7) NULL,
	[DeletedTime] [datetime2](7) NULL,
	[CreatedBy] [nvarchar](100) NULL,
	[ModifiedBy] [nvarchar](100) NULL,
	[DeletedBy] [nvarchar](100) NULL,
	[IsDeleted] [bit] NULL,
 CONSTRAINT [PK_CorrespondenceCopiedOffice] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[DLGroup]    Script Date: 4/30/2021 9:57:21 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[DLGroup](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Name] [varchar](50) NULL,
	[Description] [varchar](50) NULL,
	[ModifiedTime] [datetime2](7) NULL,
	[CreatedTime] [datetime2](7) NULL,
	[DeletedTime] [datetime2](7) NULL,
	[CreatedBy] [varchar](500) NULL,
	[ModifiedBy] [varchar](500) NULL,
	[DeletedBy] [varchar](500) NULL,
	[IsDeleted] [bit] NULL,
 CONSTRAINT [PK_ReviewGroup] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[DLGroupMembers]    Script Date: 4/30/2021 9:57:21 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[DLGroupMembers](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[DLGroupId] [int] NULL,
	[RoleId] [int] NULL,
	[UserUPN] [nvarchar](500) NULL,
	[UserFullName] [nvarchar](500) NULL,
	[ModifiedTime] [datetime2](7) NULL,
	[CreatedTime] [datetime2](7) NULL,
	[DeletedTime] [datetime2](7) NULL,
	[CreatedBy] [varchar](500) NULL,
	[ModifiedBy] [varchar](500) NULL,
	[DeletedBy] [varchar](500) NULL,
	[IsDeleted] [bit] NULL,
 CONSTRAINT [PK_DLGroupMembers] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Document]    Script Date: 4/30/2021 9:57:21 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Document](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[FolderId] [int] NOT NULL,
	[Name] [nvarchar](max) NULL,
	[Type] [nvarchar](max) NULL,
	[Path] [nvarchar](max) NULL,
	[CreatedTime] [datetime2](7) NULL,
	[ModifiedTime] [datetime2](7) NULL,
	[DeletedTime] [datetime2](7) NULL,
	[DeletedBy] [nvarchar](50) NULL,
	[CreatedBy] [nvarchar](50) NULL,
	[ModifiedBy] [nvarchar](50) NULL,
	[IsDeleted] [bit] NULL,
 CONSTRAINT [PK_Document] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[DocumentLibrary]    Script Date: 4/30/2021 9:57:21 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[DocumentLibrary](
	[Id] [int] NOT NULL,
	[Environment] [nvarchar](50) NULL,
	[Url] [nvarchar](max) NULL,
	[Description] [nvarchar](50) NULL,
	[CreatedTime] [datetime2](7) NULL,
	[ModifiedTime] [datetime2](7) NULL,
	[DeletedBy] [nvarchar](50) NULL,
	[CreatedBy] [nvarchar](50) NULL,
	[ModifiedBy] [nvarchar](50) NULL,
	[IsDeleted] [bit] NULL,
 CONSTRAINT [PK_DocumentLibrary] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[DocumentType]    Script Date: 4/30/2021 9:57:21 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[DocumentType](
	[Id] [int] NOT NULL,
	[Name] [nvarchar](50) NULL,
	[Description] [nvarchar](500) NULL,
	[CreatedTime] [datetime2](7) NULL,
	[ModifiedTime] [datetime2](7) NULL,
	[DeletedTime] [datetime2](7) NULL,
	[DeletedBy] [nvarchar](50) NULL,
	[CreatedBy] [nvarchar](50) NULL,
	[ModifiedBy] [nvarchar](50) NULL,
	[IsDeleted] [bit] NULL,
 CONSTRAINT [PK_DocumentType] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[EmailController]    Script Date: 4/30/2021 9:57:21 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[EmailController](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[FromEmailOriginator] [nchar](20) NULL,
	[ToEmailRecipients] [nchar](20) NULL,
	[CCEmailRecipients] [nchar](20) NULL,
	[BCCEmailRecipients] [nchar](20) NULL,
	[Source] [nchar](20) NULL,
	[IsEmailSent] [bit] NULL,
	[EmailMessageBody] [nvarchar](max) NULL,
	[CreatedDate] [datetime2](7) NULL,
	[CreatedBy] [nchar](20) NULL,
	[ModifiedBy] [nchar](20) NULL,
	[ModifiedDate] [datetime2](7) NULL,
	[IsEmailError] [bit] NULL,
	[EmailTemplateId] [int] NULL,
 CONSTRAINT [PK_EmailController] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[EmailNotificationLog]    Script Date: 4/30/2021 9:57:21 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[EmailNotificationLog](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[CATSID] [nvarchar](50) NULL,
	[CurrentRound] [nvarchar](500) NULL,
	[Category] [nvarchar](max) NULL,
	[IsError] [bit] NULL,
	[ErrorMessage] [nvarchar](max) NULL,
	[Status] [nvarchar](max) NULL,
	[Source] [nvarchar](max) NULL,
	[EventType] [nvarchar](max) NULL,
	[EmailTemplate] [nvarchar](max) NULL,
	[EmailOrigin] [nvarchar](max) NULL,
	[EmailRecipients] [nvarchar](max) NULL,
	[EmailCCs] [nvarchar](max) NULL,
	[EmailMessage] [nvarchar](max) NULL,
	[EmailSubject] [nvarchar](max) NULL,
	[EmailOriginNames] [nvarchar](max) NULL,
	[EmailRecipientNames] [nvarchar](max) NULL,
	[EmailCCsNames] [nvarchar](max) NULL,
	[CreatedTime] [datetime2](7) NULL,
	[ModifiedTime] [datetime2](7) NULL,
	[DeletedTime] [datetime2](7) NULL,
	[CreatedBy] [nvarchar](50) NULL,
	[ModifiedBy] [nvarchar](50) NULL,
	[DeletedBy] [nvarchar](50) NULL,
	[IsDeleted] [bit] NULL,
	[StackTrace] [nvarchar](max) NULL,
	[IsCurrentRound] [bit] NULL,
 CONSTRAINT [PK_EmailNotificationLog] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[EmailTemplate]    Script Date: 4/30/2021 9:57:21 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[EmailTemplate](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Template] [nvarchar](max) NULL,
	[Description] [nvarchar](50) NULL,
	[ExceptionLogId] [int] NULL,
 CONSTRAINT [PK_EmailTemplate] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ExceptionLog]    Script Date: 4/30/2021 9:57:21 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ExceptionLog](
	[Id] [int] NOT NULL,
	[CATSID] [nvarchar](50) NULL,
	[ExceptionSource] [nvarchar](100) NULL,
	[ExceptionSourceFile] [nvarchar](100) NULL,
	[ExceptionSourceMethod] [varchar](max) NULL,
	[ExceptionMethod] [varchar](max) NULL,
	[ExceptionStackTrace] [varchar](max) NULL,
	[ExceptionToString] [varchar](max) NULL,
	[InnerException] [varchar](max) NULL,
	[InnerExcetionToString] [varchar](max) NULL,
	[CreatedTime] [datetime2](7) NULL,
	[ModifiedTime] [datetime2](7) NULL,
	[DeletedTime] [datetime2](7) NULL,
	[CreatedBy] [nvarchar](50) NULL,
	[ModifiedBy] [nvarchar](50) NULL,
	[DeletedBy] [nvarchar](50) NULL,
	[IsDeleted] [bit] NULL,
 CONSTRAINT [PK_ExceptionLog] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ExternalUser]    Script Date: 4/30/2021 9:57:21 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ExternalUser](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Title] [nvarchar](255) NULL,
	[Name] [nvarchar](255) NULL,
	[IsDeleted] [bit] NULL,
	[CreatedBy] [nvarchar](50) NULL,
	[ModifiedBy] [nvarchar](50) NULL,
	[DeletedBy] [nvarchar](50) NULL,
	[CreatedTime] [datetime2](7) NULL,
	[DeletedTime] [datetime2](7) NULL,
	[ModifiedTime] [datetime2](7) NULL,
 CONSTRAINT [PK_ExternalUsers] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Folder]    Script Date: 4/30/2021 9:57:21 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Folder](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[CorrespondenceId] [int] NULL,
	[Name] [nvarchar](50) NULL,
	[Path] [nvarchar](max) NULL,
	[CreatedTime] [datetime2](7) NULL,
	[ModifiedTime] [datetime2](7) NULL,
	[DeletedTime] [datetime2](7) NULL,
	[DeletedBy] [nvarchar](50) NULL,
	[CreatedBy] [nvarchar](50) NULL,
	[ModifiedBy] [nvarchar](50) NULL,
	[IsDeleted] [bit] NULL,
 CONSTRAINT [PK_Folder] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[FYIUser]    Script Date: 4/30/2021 9:57:21 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[FYIUser](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[CollaborationId] [int] NULL,
	[SharepointId] [int] NULL,
	[EmailControlId] [int] NULL,
	[FYIUpn] [nvarchar](50) NULL,
	[FYIUserName] [nvarchar](50) NULL,
	[RoundName] [nvarchar](900) NULL,
	[Office] [nvarchar](50) NULL,
	[CreatedBy] [nvarchar](50) NULL,
	[ModifiedBy] [nvarchar](50) NULL,
	[DeletedBy] [nvarchar](50) NULL,
	[CreatedTime] [datetime2](7) NULL,
	[ModifiedTime] [datetime2](7) NULL,
	[DeletedTime] [datetime2](7) NULL,
	[IsDeleted] [bit] NULL,
	[IsEmailSent] [bit] NULL,
 CONSTRAINT [PK_FYIUser] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[HelpDocument]    Script Date: 4/30/2021 9:57:21 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[HelpDocument](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Title] [nvarchar](max) NULL,
	[Url] [nvarchar](max) NULL,
	[CreatedTime] [datetime2](7) NULL,
	[ModifiedTime] [datetime2](7) NULL,
	[DeletedTime] [datetime2](7) NULL,
	[CreatedBy] [nvarchar](50) NULL,
	[ModifiedBy] [nvarchar](50) NULL,
	[DeletedBy] [nvarchar](50) NULL,
	[IsDeleted] [bit] NULL,
	[IsExternal] [bit] NULL,
	[IsDevider] [bit] NULL,
	[Icon] [nvarchar](50) NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[IQ DocumentType]    Script Date: 4/30/2021 9:57:21 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[IQ DocumentType](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Title] [nvarchar](255) NULL,
	[DocumentType] [nvarchar](255) NULL,
	[DocumentDescription] [nvarchar](255) NULL,
	[CATS HTML Content] [ntext] NULL,
	[Compliance Asset Id] [nvarchar](255) NULL,
	[Content Type] [nvarchar](255) NULL,
	[App Created By] [nvarchar](255) NULL,
	[App Modified By] [nvarchar](255) NULL,
	[Attachments] [ntext] NULL,
	[Workflow Instance ID] [nvarchar](255) NULL,
	[File Type] [nvarchar](255) NULL,
	[Modified] [datetime] NULL,
	[Created] [datetime] NULL,
	[Created By] [int] NULL,
	[Modified By] [int] NULL,
	[URL Path] [nvarchar](255) NULL,
	[Path] [nvarchar](255) NULL,
	[Item Type] [nvarchar](255) NULL,
	[Encoded Absolute URL] [nvarchar](255) NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[IQ History List]    Script Date: 4/30/2021 9:57:21 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[IQ History List](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[CATS ID] [nvarchar](255) NULL,
	[Review Package Work Area] [nvarchar](255) NULL,
	[Review Package Name] [nvarchar](255) NULL,
	[CATS User Type] [nvarchar](255) NULL,
	[Action] [nvarchar](255) NULL,
	[Request Status] [nvarchar](255) NULL,
	[Failed To Process Explanation] [ntext] NULL,
	[Notification Status] [nvarchar](255) NULL,
	[Action Description] [nvarchar](255) NULL,
	[CATS-IQ-HISTORY-LIST-ON-ITEM-CREATED] [nvarchar](255) NULL,
	[CATS-IQ-HISTORY-LIST-ON-ITEM-CREATED1] [nvarchar](255) NULL,
	[Compliance Asset Id] [nvarchar](255) NULL,
	[Content Type] [nvarchar](255) NULL,
	[App Created By] [nvarchar](255) NULL,
	[App Modified By] [nvarchar](255) NULL,
	[Users] [ntext] NULL,
	[Additional Users] [ntext] NULL,
	[Attachments] [ntext] NULL,
	[Workflow Instance ID] [nvarchar](255) NULL,
	[File Type] [nvarchar](255) NULL,
	[Modified] [datetime] NULL,
	[Created] [datetime] NULL,
	[Created By] [int] NULL,
	[Modified By] [int] NULL,
	[URL Path] [nvarchar](255) NULL,
	[Path] [nvarchar](255) NULL,
	[Item Type] [nvarchar](255) NULL,
	[Encoded Absolute URL] [nvarchar](255) NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[LeadOffice]    Script Date: 4/30/2021 9:57:21 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[LeadOffice](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](50) NULL,
	[Description] [nvarchar](500) NULL,
	[IsDeleted] [bit] NULL,
	[CreatedBy] [nvarchar](50) NULL,
	[ModifiedBy] [nvarchar](50) NULL,
	[DeletedBy] [nvarchar](50) NULL,
	[CreatedTime] [datetime2](7) NULL,
	[ModifiedTime] [datetime2](7) NULL,
	[DeletedTime] [datetime2](7) NULL,
	[IsHidden] [bit] NULL,
 CONSTRAINT [PK_LeadOffice] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[LeadOfficeMember]    Script Date: 4/30/2021 9:57:21 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[LeadOfficeMember](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[LeadOfficeId] [int] NULL,
	[ExternalLeadOfficeIds] [nvarchar](500) NULL,
	[RoleId] [int] NULL,
	[UserUPN] [nvarchar](500) NULL,
	[UserUPN_SP] [nvarchar](500) NULL,
	[UserFullName] [nvarchar](500) NULL,
	[ModifiedTime] [datetime2](7) NULL,
	[CreatedTime] [datetime2](7) NULL,
	[DeletedTime] [datetime2](7) NULL,
	[CreatedBy] [varchar](500) NULL,
	[ModifiedBy] [varchar](500) NULL,
	[DeletedBy] [varchar](500) NULL,
	[IsDeleted] [bit] NULL,
 CONSTRAINT [PK_LeadOfficeMember] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[LeadOfficeOfficeManager]    Script Date: 4/30/2021 9:57:21 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[LeadOfficeOfficeManager](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[LeadOfficeId] [int] NULL,
	[ExternalLeadOfficeIds] [nvarchar](500) NULL,
	[RoleId] [int] NULL,
	[UserUPN] [nvarchar](500) NULL,
	[UserUPN_SP] [nvarchar](500) NULL,
	[UserFullName] [nvarchar](500) NULL,
	[ModifiedTime] [datetime2](7) NULL,
	[CreatedTime] [datetime2](7) NULL,
	[DeletedTime] [datetime2](7) NULL,
	[CreatedBy] [varchar](500) NULL,
	[ModifiedBy] [varchar](500) NULL,
	[DeletedBy] [varchar](500) NULL,
	[IsDeleted] [bit] NULL,
 CONSTRAINT [PK_LeadOfficeOfficeManager] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[LetterType]    Script Date: 4/30/2021 9:57:21 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[LetterType](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](50) NULL,
	[Description] [nvarchar](50) NULL,
	[IsDeleted] [bit] NULL,
	[CreatedBy] [nvarchar](50) NULL,
	[ModifiedBy] [nvarchar](50) NULL,
	[DeletedBy] [nvarchar](50) NULL,
	[CreatedTime] [datetime2](7) NULL,
	[DeletedTime] [datetime2](7) NULL,
	[ModifiedTime] [datetime2](7) NULL,
	[HtmlContent] [nvarchar](max) NULL,
 CONSTRAINT [PK_LetterType] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[MigratedFiles]    Script Date: 4/30/2021 9:57:21 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MigratedFiles](
	[MigratedFiles] [nvarchar](max) NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[OldIQMasterList]    Script Date: 4/30/2021 9:57:21 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[OldIQMasterList](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[CATSID] [nvarchar](255) NULL,
	[CurrentReview] [nvarchar](255) NULL,
	[IQStatus] [nvarchar](255) NULL,
	[FinalDocuments] [ntext] NULL,
	[ReferenceDocuments] [ntext] NULL,
	[ReviewDocuments] [ntext] NULL,
	[FolderUrlPath] [nvarchar](255) NULL,
	[FolderName] [nvarchar](255) NULL,
	[FolderID] [int] NULL,
	[AttachedDocumentsOriginalNames] [ntext] NULL,
	[SignedFinalResponsePDFFileName] [ntext] NULL,
	[CreatedBy] [nvarchar](50) NULL,
	[CreatedByUpn] [nvarchar](50) NULL,
	[ModifiedBy] [nvarchar](50) NULL,
	[ModifiedByUpn] [nvarchar](50) NULL,
	[DeletedBy] [nvarchar](50) NULL,
	[ModifiedTime] [datetime2](7) NULL,
	[CreatedTime] [datetime2](7) NULL,
	[DeletedTime] [datetime2](7) NULL,
	[IsDeleted] [bit] NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Originator]    Script Date: 4/30/2021 9:57:21 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Originator](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[RoundName] [nvarchar](900) NULL,
	[CollaborationId] [int] NULL,
	[EmailControlId] [int] NULL,
	[OriginatorUpn] [nvarchar](200) NOT NULL,
	[SharepointId] [int] NULL,
	[OriginatorName] [nvarchar](500) NULL,
	[Office] [nvarchar](500) NULL,
	[CreatedTime] [datetime2](7) NULL,
	[ModifiedTime] [datetime2](7) NULL,
	[DeletedTime] [datetime2](7) NULL,
	[CreatedBy] [nvarchar](500) NULL,
	[ModifiedBy] [nvarchar](500) NULL,
	[DeletedBy] [nvarchar](500) NULL,
	[IsDeleted] [bit] NULL,
	[IsEmailSent] [bit] NULL,
 CONSTRAINT [PK_Originator] PRIMARY KEY CLUSTERED 
(
	[Id] ASC,
	[OriginatorUpn] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Reviewer]    Script Date: 4/30/2021 9:57:21 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Reviewer](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[CollaborationId] [int] NULL,
	[RoundName] [nvarchar](900) NULL,
	[EmailControlId] [int] NULL,
	[ReviewerUPN] [nvarchar](200) NULL,
	[SharepointId] [int] NULL,
	[ReviewerName] [nvarchar](500) NULL,
	[Office] [nvarchar](500) NULL,
	[RoundStartDate] [datetime2](7) NULL,
	[RoundEndDate] [datetime2](7) NULL,
	[RoundCompletedDate] [datetime2](7) NULL,
	[RoundCompletedBy] [nvarchar](500) NULL,
	[RoundCompletedByUpn] [nvarchar](500) NULL,
	[IsRoundCompletedBySurrogate] [bit] NULL,
	[RoundActivity] [nvarchar](500) NULL,
	[DraftBy] [nvarchar](500) NULL,
	[DraftByUpn] [nvarchar](500) NULL,
	[DraftReason] [nvarchar](max) NULL,
	[DraftDate] [datetime2](7) NULL,
	[CreatedTime] [datetime2](7) NULL,
	[ModifiedTime] [datetime2](7) NULL,
	[DeletedTime] [datetime2](7) NULL,
	[CreatedBy] [nvarchar](500) NULL,
	[ModifiedBy] [nvarchar](500) NULL,
	[DeletedBy] [nvarchar](500) NULL,
	[IsDeleted] [bit] NULL,
	[SurrogateFullName] [nvarchar](500) NULL,
	[SurrogateUpn] [nvarchar](500) NULL,
	[IsEmailSent] [bit] NULL,
	[IsCurrentRound] [bit] NULL,
 CONSTRAINT [PK_Reviewer] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ReviewRound]    Script Date: 4/30/2021 9:57:21 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ReviewRound](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](50) NULL,
	[ReviewRoundAcronym] [nvarchar](50) NULL,
	[Description] [nvarchar](max) NULL,
	[IsCombinedRounds] [bit] NULL,
	[CreatedBy] [nvarchar](100) NULL,
	[ModifiedBy] [nvarchar](100) NULL,
	[DeletedBy] [nvarchar](100) NULL,
	[CreatedTime] [datetime2](7) NULL,
	[ModifiedTime] [datetime2](7) NULL,
	[DeletedTime] [datetime2](7) NULL,
	[IsDeleted] [bit] NULL,
 CONSTRAINT [PK_ReviewRound] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Role]    Script Date: 4/30/2021 9:57:21 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Role](
	[Id] [int] NOT NULL,
	[Name] [nvarchar](50) NULL,
	[Description] [nvarchar](500) NULL,
	[CreatedBy] [nvarchar](50) NULL,
	[ModifiedBy] [nvarchar](50) NULL,
	[DeletedBy] [nvarchar](50) NULL,
	[CreatedTime] [datetime2](7) NULL,
	[ModifiedTime] [datetime2](7) NULL,
	[DeletedTime] [datetime2](7) NULL,
	[IsDeleted] [bit] NULL,
 CONSTRAINT [PK_Roles] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[SurrogateOriginator]    Script Date: 4/30/2021 9:57:21 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[SurrogateOriginator](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[CATSUserSPID] [varchar](100) NULL,
	[CATSUser] [varchar](100) NULL,
	[CATSUserUPN] [nvarchar](200) NOT NULL,
	[SurrogateSPUserID] [nvarchar](255) NULL,
	[Surrogate] [varchar](100) NULL,
	[SurrogateUPN] [nvarchar](255) NULL,
	[CreatedTime] [datetime2](7) NULL,
	[ModifiedTime] [datetime2](7) NULL,
	[DeletedTime] [datetime2](7) NULL,
	[CreatedBy] [varchar](100) NULL,
	[ModifiedBy] [varchar](100) NULL,
	[IsDeleted] [bit] NULL,
	[DeletedBy] [varchar](100) NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[SurrogateReviewer]    Script Date: 4/30/2021 9:57:21 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[SurrogateReviewer](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[CATSUserSPID] [varchar](100) NULL,
	[CATSUser] [varchar](100) NULL,
	[CATSUserUPN] [nvarchar](200) NOT NULL,
	[SurrogateSPUserID] [nvarchar](255) NULL,
	[Surrogate] [varchar](100) NULL,
	[SurrogateUPN] [nvarchar](255) NULL,
	[CreatedTime] [datetime2](7) NULL,
	[ModifiedTime] [datetime2](7) NULL,
	[DeletedTime] [datetime2](7) NULL,
	[CreatedBy] [varchar](100) NULL,
	[ModifiedBy] [varchar](100) NULL,
	[IsDeleted] [bit] NULL,
	[DeletedBy] [varchar](100) NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Users]    Script Date: 4/30/2021 9:57:21 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Users](
	[Id] [uniqueidentifier] NOT NULL,
	[Name] [nvarchar](max) NULL,
	[Address] [nvarchar](max) NULL,
	[ImgPath] [nvarchar](max) NULL,
 CONSTRAINT [PK_User] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [idx_CATSID]    Script Date: 4/30/2021 9:57:21 AM ******/
CREATE NONCLUSTERED INDEX [idx_CATSID] ON [dbo].[Correspondence]
(
	[CATSID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [NonClusteredIndex-20210309-145433]    Script Date: 4/30/2021 9:57:21 AM ******/
CREATE NONCLUSTERED INDEX [NonClusteredIndex-20210309-145433] ON [dbo].[Correspondence]
(
	[LeadOfficeName] ASC,
	[LetterTypeName] ASC,
	[LetterStatus] ASC,
	[ReviewStatus] ASC,
	[CurrentReview] ASC,
	[LetterCrossReference] ASC,
	[LetterDate] ASC,
	[LetterReceiptDate] ASC,
	[DueforSignatureByDate] ASC,
	[PADDueDate] ASC,
	[FiscalYear] ASC,
	[IsUnderReview] ASC,
	[CreatedTime] ASC,
	[ModifiedTime] ASC,
	[CreatedBy] ASC,
	[ModifiedBy] ASC,
	[IsDeleted] ASC,
	[SurrogateFullName] ASC,
	[SurrogateUpn] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [idx_EmailNotificationLogCATSID]    Script Date: 4/30/2021 9:57:21 AM ******/
CREATE NONCLUSTERED INDEX [idx_EmailNotificationLogCATSID] ON [dbo].[EmailNotificationLog]
(
	[CATSID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [idx_EmailNotificationLogCurrentRound]    Script Date: 4/30/2021 9:57:21 AM ******/
CREATE NONCLUSTERED INDEX [idx_EmailNotificationLogCurrentRound] ON [dbo].[EmailNotificationLog]
(
	[CurrentRound] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [NonClusteredIndex-20210304-153850]    Script Date: 4/30/2021 9:57:21 AM ******/
CREATE NONCLUSTERED INDEX [NonClusteredIndex-20210304-153850] ON [dbo].[FYIUser]
(
	[FYIUpn] ASC,
	[FYIUserName] ASC,
	[RoundName] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
ALTER TABLE [dbo].[Administration] ADD  CONSTRAINT [DF_Administration_IsCurrent]  DEFAULT ((0)) FOR [IsCurrent]
GO
ALTER TABLE [dbo].[Administration] ADD  CONSTRAINT [DF_Administration_CreatedBy]  DEFAULT ('CATS Admin') FOR [CreatedBy]
GO
ALTER TABLE [dbo].[Administration] ADD  CONSTRAINT [DF_Administration_ModifiedBy]  DEFAULT ('CATS Admin') FOR [ModifiedBy]
GO
ALTER TABLE [dbo].[Administration] ADD  CONSTRAINT [DF_Administration_CreatedTime]  DEFAULT (getdate()) FOR [CreatedTime]
GO
ALTER TABLE [dbo].[Administration] ADD  CONSTRAINT [DF_Administration_ModifiedTime]  DEFAULT (getdate()) FOR [ModifiedTime]
GO
ALTER TABLE [dbo].[Administration] ADD  CONSTRAINT [DF_Administration_IsDeleted]  DEFAULT ((0)) FOR [IsDeleted]
GO
ALTER TABLE [dbo].[Collaboration] ADD  CONSTRAINT [DF_Collaboration_IsDeleted]  DEFAULT ((0)) FOR [IsDeleted]
GO
ALTER TABLE [dbo].[CompletedReviewRound] ADD  CONSTRAINT [DF_CompletedReviewRound_CreatedBy]  DEFAULT (N'CATS Admin') FOR [CreatedBy]
GO
ALTER TABLE [dbo].[CompletedReviewRound] ADD  CONSTRAINT [DF_CompletedReviewRound_CreatedTime]  DEFAULT (getdate()) FOR [CreatedTime]
GO
ALTER TABLE [dbo].[CompletedReviewRound] ADD  CONSTRAINT [DF_CompletedReviewRound_ModifiedBy]  DEFAULT (N'CATS Admin') FOR [ModifiedBy]
GO
ALTER TABLE [dbo].[CompletedReviewRound] ADD  CONSTRAINT [DF_CompletedReviewRound_ModifiedTime]  DEFAULT (getdate()) FOR [ModifiedTime]
GO
ALTER TABLE [dbo].[Correspondence] ADD  CONSTRAINT [DF_Correspondence_AdministrationId]  DEFAULT ([dbo].[ufnGenerateAdministrationId]()) FOR [AdministrationId]
GO
ALTER TABLE [dbo].[Correspondence] ADD  CONSTRAINT [DF_Correspondence_CATSID]  DEFAULT ([dbo].[ufnGenerateCATSID]()) FOR [CATSID]
GO
ALTER TABLE [dbo].[Correspondence] ADD  CONSTRAINT [DF_Correspondence_LetterStatus]  DEFAULT (N'Open') FOR [LetterStatus]
GO
ALTER TABLE [dbo].[Correspondence] ADD  CONSTRAINT [DF_Correspondence_ReviewStatus]  DEFAULT (N'NONE') FOR [ReviewStatus]
GO
ALTER TABLE [dbo].[Correspondence] ADD  CONSTRAINT [DF_Correspondence_CurrentReview]  DEFAULT (N'NONE') FOR [CurrentReview]
GO
ALTER TABLE [dbo].[Correspondence] ADD  CONSTRAINT [DF_Correspondence_IsLetterRequired]  DEFAULT ((1)) FOR [IsLetterRequired]
GO
ALTER TABLE [dbo].[Correspondence] ADD  CONSTRAINT [DF_Correspondence_FiscalYear]  DEFAULT (datepart(year,getdate())) FOR [FiscalYear]
GO
ALTER TABLE [dbo].[Correspondence] ADD  CONSTRAINT [DF_Correspondence_IsPendingLeadOffice]  DEFAULT ((0)) FOR [IsPendingLeadOffice]
GO
ALTER TABLE [dbo].[Correspondence] ADD  CONSTRAINT [DF_Correspondence_IsUnderReview]  DEFAULT ((0)) FOR [IsUnderReview]
GO
ALTER TABLE [dbo].[Correspondence] ADD  CONSTRAINT [DF_Correspondence_IsReopen]  DEFAULT ((0)) FOR [IsReopen]
GO
ALTER TABLE [dbo].[Correspondence] ADD  CONSTRAINT [DF_Correspondence_IsAdminReOpen]  DEFAULT ((0)) FOR [IsAdminReOpen]
GO
ALTER TABLE [dbo].[Correspondence] ADD  CONSTRAINT [DF_Correspondence_IsAdminClosure]  DEFAULT ((0)) FOR [IsAdminClosure]
GO
ALTER TABLE [dbo].[Correspondence] ADD  CONSTRAINT [DF_Correspondence_CreatedDate]  DEFAULT (getdate()) FOR [CreatedDate]
GO
ALTER TABLE [dbo].[Correspondence] ADD  CONSTRAINT [DF_Correspondence_ModifiedDate]  DEFAULT (getdate()) FOR [ModifiedDate]
GO
ALTER TABLE [dbo].[Correspondence] ADD  CONSTRAINT [DF_Correspondence_CreatedTime]  DEFAULT (getdate()) FOR [CreatedTime]
GO
ALTER TABLE [dbo].[Correspondence] ADD  CONSTRAINT [DF_Correspondence_ModifiedTime]  DEFAULT (getdate()) FOR [ModifiedTime]
GO
ALTER TABLE [dbo].[Correspondence] ADD  CONSTRAINT [DF_Correspondence_IsDeleted]  DEFAULT ((0)) FOR [IsDeleted]
GO
ALTER TABLE [dbo].[Correspondence] ADD  CONSTRAINT [DF_Correspondence_Deleted]  DEFAULT ((0)) FOR [Deleted]
GO
ALTER TABLE [dbo].[Correspondence] ADD  CONSTRAINT [DF_Correspondence_IsSaved]  DEFAULT ((0)) FOR [IsSaved]
GO
ALTER TABLE [dbo].[Correspondence] ADD  CONSTRAINT [DF_Correspondence_IsArchived]  DEFAULT ((0)) FOR [IsArchived]
GO
ALTER TABLE [dbo].[Correspondence] ADD  CONSTRAINT [DF_Correspondence_IsEmailElligible]  DEFAULT ((1)) FOR [IsEmailElligible]
GO
ALTER TABLE [dbo].[CorrespondenceCopiedArchived] ADD  CONSTRAINT [DF_CorrespondenceCopiedArchived_CreatedTime]  DEFAULT (getdate()) FOR [CreatedTime]
GO
ALTER TABLE [dbo].[CorrespondenceCopiedArchived] ADD  CONSTRAINT [DF_CorrespondenceCopiedArchived_ModifiedTime]  DEFAULT (getdate()) FOR [ModifiedTime]
GO
ALTER TABLE [dbo].[CorrespondenceCopiedArchived] ADD  CONSTRAINT [DF_CorrespondenceCopiedArchived_CreatedBy]  DEFAULT (N'CATS Admin') FOR [CreatedBy]
GO
ALTER TABLE [dbo].[CorrespondenceCopiedArchived] ADD  CONSTRAINT [DF_CorrespondenceCopiedArchived_ModifiedBy]  DEFAULT (N'CATS Admin') FOR [ModifiedBy]
GO
ALTER TABLE [dbo].[CorrespondenceCopiedArchived] ADD  CONSTRAINT [DF_CorrespondenceCopiedArchived_IsDeleted]  DEFAULT ((0)) FOR [IsDeleted]
GO
ALTER TABLE [dbo].[CorrespondenceCopiedOffice] ADD  CONSTRAINT [DF_CorrespondenceCopiedOffice_CreatedTime]  DEFAULT (getdate()) FOR [CreatedTime]
GO
ALTER TABLE [dbo].[CorrespondenceCopiedOffice] ADD  CONSTRAINT [DF_CorrespondenceCopiedOffice_ModifiedTime]  DEFAULT (getdate()) FOR [ModifiedTime]
GO
ALTER TABLE [dbo].[CorrespondenceCopiedOffice] ADD  CONSTRAINT [DF_CorrespondenceCopiedOffice_CreatedBy]  DEFAULT (N'CATS Admin') FOR [CreatedBy]
GO
ALTER TABLE [dbo].[CorrespondenceCopiedOffice] ADD  CONSTRAINT [DF_CorrespondenceCopiedOffice_ModifiedBy]  DEFAULT (N'CATS Admin') FOR [ModifiedBy]
GO
ALTER TABLE [dbo].[CorrespondenceCopiedOffice] ADD  CONSTRAINT [DF_CorrespondenceCopiedOffice_IsDeleted]  DEFAULT ((0)) FOR [IsDeleted]
GO
ALTER TABLE [dbo].[DLGroup] ADD  CONSTRAINT [DF_DLGroup_ModifiedTime]  DEFAULT (getdate()) FOR [ModifiedTime]
GO
ALTER TABLE [dbo].[DLGroup] ADD  CONSTRAINT [DF_DLGroup_CreatedTime]  DEFAULT (getdate()) FOR [CreatedTime]
GO
ALTER TABLE [dbo].[DLGroup] ADD  CONSTRAINT [DF_DLGroup_CreatedBy]  DEFAULT ('CATS Admin') FOR [CreatedBy]
GO
ALTER TABLE [dbo].[DLGroup] ADD  CONSTRAINT [DF_DLGroup_ModifiedBy]  DEFAULT ('CATS Admin') FOR [ModifiedBy]
GO
ALTER TABLE [dbo].[DLGroup] ADD  CONSTRAINT [DF_DLGroup_IsDeleted]  DEFAULT ((0)) FOR [IsDeleted]
GO
ALTER TABLE [dbo].[DLGroupMembers] ADD  CONSTRAINT [DF_DLGroupMembers_ModifiedTime]  DEFAULT (getdate()) FOR [ModifiedTime]
GO
ALTER TABLE [dbo].[DLGroupMembers] ADD  CONSTRAINT [DF_DLGroupMembers_CreatedTime]  DEFAULT (getdate()) FOR [CreatedTime]
GO
ALTER TABLE [dbo].[DLGroupMembers] ADD  CONSTRAINT [DF_DLGroupMembers_CreatedBy]  DEFAULT ('CATS Admin') FOR [CreatedBy]
GO
ALTER TABLE [dbo].[DLGroupMembers] ADD  CONSTRAINT [DF_DLGroupMembers_ModifiedBy]  DEFAULT ('CATS Admin') FOR [ModifiedBy]
GO
ALTER TABLE [dbo].[DLGroupMembers] ADD  CONSTRAINT [DF_DLGroupMembers_IsDeleted]  DEFAULT ((0)) FOR [IsDeleted]
GO
ALTER TABLE [dbo].[Document] ADD  CONSTRAINT [DF_Document_CreatedTime]  DEFAULT (getdate()) FOR [CreatedTime]
GO
ALTER TABLE [dbo].[Document] ADD  CONSTRAINT [DF_Document_ModifiedTime]  DEFAULT (getdate()) FOR [ModifiedTime]
GO
ALTER TABLE [dbo].[Document] ADD  CONSTRAINT [DF_Document_IsDeleted]  DEFAULT ((0)) FOR [IsDeleted]
GO
ALTER TABLE [dbo].[DocumentLibrary] ADD  CONSTRAINT [DF_DocumentLibrary_IsDeleted]  DEFAULT ((0)) FOR [IsDeleted]
GO
ALTER TABLE [dbo].[DocumentType] ADD  CONSTRAINT [DF_DocumentType_CreatedTime]  DEFAULT (getdate()) FOR [CreatedTime]
GO
ALTER TABLE [dbo].[DocumentType] ADD  CONSTRAINT [DF_DocumentType_ModifiedTime]  DEFAULT (getdate()) FOR [ModifiedTime]
GO
ALTER TABLE [dbo].[DocumentType] ADD  CONSTRAINT [DF_DocumentType_CreatedBy]  DEFAULT (N'CATS Admin') FOR [CreatedBy]
GO
ALTER TABLE [dbo].[DocumentType] ADD  CONSTRAINT [DF_DocumentType_ModifiedBy]  DEFAULT (N'CATS Admin') FOR [ModifiedBy]
GO
ALTER TABLE [dbo].[DocumentType] ADD  CONSTRAINT [DF_DocumentType_IsDeleted]  DEFAULT ((0)) FOR [IsDeleted]
GO
ALTER TABLE [dbo].[EmailNotificationLog] ADD  CONSTRAINT [DF_EmailNotificationLog_CreatedTime]  DEFAULT (getdate()) FOR [CreatedTime]
GO
ALTER TABLE [dbo].[EmailNotificationLog] ADD  CONSTRAINT [DF_EmailNotificationLog_ModifiedTime]  DEFAULT (getdate()) FOR [ModifiedTime]
GO
ALTER TABLE [dbo].[EmailNotificationLog] ADD  CONSTRAINT [DF_EmailNotificationLog_CreatedBy]  DEFAULT (N'CATS Admin') FOR [CreatedBy]
GO
ALTER TABLE [dbo].[EmailNotificationLog] ADD  CONSTRAINT [DF_EmailNotificationLog_ModifiedBy]  DEFAULT (N'CATS Admin') FOR [ModifiedBy]
GO
ALTER TABLE [dbo].[EmailNotificationLog] ADD  CONSTRAINT [DF_EmailNotificationLog_IsDeleted]  DEFAULT ((0)) FOR [IsDeleted]
GO
ALTER TABLE [dbo].[EmailNotificationLog] ADD  CONSTRAINT [DF_EmailNotificationLog_IsCurrentRound]  DEFAULT ((1)) FOR [IsCurrentRound]
GO
ALTER TABLE [dbo].[ExceptionLog] ADD  CONSTRAINT [DF_ExceptionLog_CreatedTime]  DEFAULT (getdate()) FOR [CreatedTime]
GO
ALTER TABLE [dbo].[ExceptionLog] ADD  CONSTRAINT [DF_ExceptionLog_ModifiedTime]  DEFAULT (getdate()) FOR [ModifiedTime]
GO
ALTER TABLE [dbo].[ExceptionLog] ADD  CONSTRAINT [DF_ExceptionLog_IsDeleted]  DEFAULT ((0)) FOR [IsDeleted]
GO
ALTER TABLE [dbo].[ExternalUser] ADD  CONSTRAINT [DF_ExternalUsers_IsDeleted]  DEFAULT ((0)) FOR [IsDeleted]
GO
ALTER TABLE [dbo].[ExternalUser] ADD  CONSTRAINT [DF_ExternalUsers_CreatedBy]  DEFAULT (N'CATS Admin') FOR [CreatedBy]
GO
ALTER TABLE [dbo].[ExternalUser] ADD  CONSTRAINT [DF_ExternalUsers_ModifiedBy]  DEFAULT (N'CATS Admin') FOR [ModifiedBy]
GO
ALTER TABLE [dbo].[ExternalUser] ADD  CONSTRAINT [DF_ExternalUsers_DeletedBy]  DEFAULT (N'CATS Admin') FOR [DeletedBy]
GO
ALTER TABLE [dbo].[ExternalUser] ADD  CONSTRAINT [DF_ExternalUser_CreatedTime]  DEFAULT (getdate()) FOR [CreatedTime]
GO
ALTER TABLE [dbo].[ExternalUser] ADD  CONSTRAINT [DF_ExternalUser_ModifiedTime]  DEFAULT (getdate()) FOR [ModifiedTime]
GO
ALTER TABLE [dbo].[Folder] ADD  CONSTRAINT [DF_Folder_IsDeleted]  DEFAULT ((0)) FOR [IsDeleted]
GO
ALTER TABLE [dbo].[FYIUser] ADD  CONSTRAINT [DF_FYIUser_CreatedBy]  DEFAULT (N'CATS Admin') FOR [CreatedBy]
GO
ALTER TABLE [dbo].[FYIUser] ADD  CONSTRAINT [DF_FYIUser_ModifiedBy]  DEFAULT (N'CATS Admin') FOR [ModifiedBy]
GO
ALTER TABLE [dbo].[FYIUser] ADD  CONSTRAINT [DF_FYIUser_CreatedTime]  DEFAULT (getdate()) FOR [CreatedTime]
GO
ALTER TABLE [dbo].[FYIUser] ADD  CONSTRAINT [DF_FYIUser_ModifiedTime]  DEFAULT (getdate()) FOR [ModifiedTime]
GO
ALTER TABLE [dbo].[FYIUser] ADD  CONSTRAINT [DF_FYIUser_IsDeleted]  DEFAULT ((0)) FOR [IsDeleted]
GO
ALTER TABLE [dbo].[FYIUser] ADD  CONSTRAINT [DF_FYIUser_IsEmailSent]  DEFAULT ((0)) FOR [IsEmailSent]
GO
ALTER TABLE [dbo].[HelpDocument] ADD  CONSTRAINT [DF_HelpDocument_CreateTime]  DEFAULT (getdate()) FOR [CreatedTime]
GO
ALTER TABLE [dbo].[HelpDocument] ADD  CONSTRAINT [DF_HelpDocument_ModifiedTime]  DEFAULT (getdate()) FOR [ModifiedTime]
GO
ALTER TABLE [dbo].[HelpDocument] ADD  CONSTRAINT [DF_HelpDocument_CreatedBy]  DEFAULT (N'CATS Admin') FOR [CreatedBy]
GO
ALTER TABLE [dbo].[HelpDocument] ADD  CONSTRAINT [DF_HelpDocument_ModifiedBy]  DEFAULT (N'CATS Admin') FOR [ModifiedBy]
GO
ALTER TABLE [dbo].[HelpDocument] ADD  CONSTRAINT [DF_HelpDocument_IsDeleted]  DEFAULT ((0)) FOR [IsDeleted]
GO
ALTER TABLE [dbo].[HelpDocument] ADD  CONSTRAINT [DF_HelpDocument_IsExternal]  DEFAULT ((0)) FOR [IsExternal]
GO
ALTER TABLE [dbo].[HelpDocument] ADD  CONSTRAINT [DF_HelpDocument_IsDevider]  DEFAULT ((0)) FOR [IsDevider]
GO
ALTER TABLE [dbo].[HelpDocument] ADD  CONSTRAINT [DF_HelpDocument_Icon]  DEFAULT (N'text_snippet') FOR [Icon]
GO
ALTER TABLE [dbo].[LeadOffice] ADD  CONSTRAINT [DF_LeadOffice_IsDeleted]  DEFAULT ((0)) FOR [IsDeleted]
GO
ALTER TABLE [dbo].[LeadOffice] ADD  CONSTRAINT [DF_LeadOffice_CreatedBy]  DEFAULT (N'CATS Admin') FOR [CreatedBy]
GO
ALTER TABLE [dbo].[LeadOffice] ADD  CONSTRAINT [DF_LeadOffice_ModifiedBy]  DEFAULT (N'CATS Admin') FOR [ModifiedBy]
GO
ALTER TABLE [dbo].[LeadOffice] ADD  CONSTRAINT [DF_LeadOffice_DeletedBy]  DEFAULT (N'CATS Admin') FOR [DeletedBy]
GO
ALTER TABLE [dbo].[LeadOffice] ADD  CONSTRAINT [DF_LeadOffice_CreatedTime]  DEFAULT (getdate()) FOR [CreatedTime]
GO
ALTER TABLE [dbo].[LeadOffice] ADD  CONSTRAINT [DF_LeadOffice_ModifiedTime]  DEFAULT (getdate()) FOR [ModifiedTime]
GO
ALTER TABLE [dbo].[LeadOffice] ADD  CONSTRAINT [DF_LeadOffice_IsHidden]  DEFAULT ((0)) FOR [IsHidden]
GO
ALTER TABLE [dbo].[LeadOfficeMember] ADD  CONSTRAINT [DF_LeadOfficeMember_ModifiedTime]  DEFAULT (getdate()) FOR [ModifiedTime]
GO
ALTER TABLE [dbo].[LeadOfficeMember] ADD  CONSTRAINT [DF_LeadOfficeMember_CreatedTime]  DEFAULT (getdate()) FOR [CreatedTime]
GO
ALTER TABLE [dbo].[LeadOfficeMember] ADD  CONSTRAINT [DF_LeadOfficeMember_CreatedBy]  DEFAULT ('CATS Admin') FOR [CreatedBy]
GO
ALTER TABLE [dbo].[LeadOfficeMember] ADD  CONSTRAINT [DF_LeadOfficeMemberr_ModifiedBy]  DEFAULT ('CATS Admin') FOR [ModifiedBy]
GO
ALTER TABLE [dbo].[LeadOfficeMember] ADD  CONSTRAINT [DF_LeadOfficeMember_IsDeleted]  DEFAULT ((0)) FOR [IsDeleted]
GO
ALTER TABLE [dbo].[LeadOfficeOfficeManager] ADD  CONSTRAINT [DF_LeadOfficeOfficeManager_ModifiedTime]  DEFAULT (getdate()) FOR [ModifiedTime]
GO
ALTER TABLE [dbo].[LeadOfficeOfficeManager] ADD  CONSTRAINT [DF_LeadOfficeOfficeManager_CreatedTime]  DEFAULT (getdate()) FOR [CreatedTime]
GO
ALTER TABLE [dbo].[LeadOfficeOfficeManager] ADD  CONSTRAINT [DF_LeadOfficeOfficeManager_CreatedBy]  DEFAULT ('CATS Admin') FOR [CreatedBy]
GO
ALTER TABLE [dbo].[LeadOfficeOfficeManager] ADD  CONSTRAINT [DF_LeadOfficeOfficeManager_ModifiedBy]  DEFAULT ('CATS Admin') FOR [ModifiedBy]
GO
ALTER TABLE [dbo].[LeadOfficeOfficeManager] ADD  CONSTRAINT [DF_LeadOfficeOfficeManager_IsDeleted]  DEFAULT ((0)) FOR [IsDeleted]
GO
ALTER TABLE [dbo].[LetterType] ADD  CONSTRAINT [DF_LetterType_IsDeleted]  DEFAULT ((0)) FOR [IsDeleted]
GO
ALTER TABLE [dbo].[LetterType] ADD  CONSTRAINT [DF_LetterType_CreatedBy]  DEFAULT (N'CATS Admin') FOR [CreatedBy]
GO
ALTER TABLE [dbo].[LetterType] ADD  CONSTRAINT [DF_LetterType_ModifiedBy]  DEFAULT (N'CATS Admin') FOR [ModifiedBy]
GO
ALTER TABLE [dbo].[LetterType] ADD  CONSTRAINT [DF_LetterType_DeletedBy]  DEFAULT (N'CATS Admin') FOR [DeletedBy]
GO
ALTER TABLE [dbo].[LetterType] ADD  CONSTRAINT [DF_LetterType_CreatedTime]  DEFAULT (getdate()) FOR [CreatedTime]
GO
ALTER TABLE [dbo].[LetterType] ADD  CONSTRAINT [DF_LetterType_ModifiedTime]  DEFAULT (getdate()) FOR [ModifiedTime]
GO
ALTER TABLE [dbo].[OldIQMasterList] ADD  CONSTRAINT [DF_OldIQMasterList_CreatedBy]  DEFAULT (N'CATS Admin') FOR [CreatedBy]
GO
ALTER TABLE [dbo].[OldIQMasterList] ADD  CONSTRAINT [DF_OldIQMasterList_ModifiedBy]  DEFAULT (N'CATS Admin') FOR [ModifiedBy]
GO
ALTER TABLE [dbo].[OldIQMasterList] ADD  CONSTRAINT [DF_OldIQMasterList_ModifiedTime]  DEFAULT (getdate()) FOR [ModifiedTime]
GO
ALTER TABLE [dbo].[OldIQMasterList] ADD  CONSTRAINT [DF_OldIQMasterList_CreatedTime]  DEFAULT (getdate()) FOR [CreatedTime]
GO
ALTER TABLE [dbo].[OldIQMasterList] ADD  CONSTRAINT [DF_OldIQMasterList_IsDeleted]  DEFAULT ((0)) FOR [IsDeleted]
GO
ALTER TABLE [dbo].[Originator] ADD  CONSTRAINT [DF_Originator_CreatedTime]  DEFAULT (getdate()) FOR [CreatedTime]
GO
ALTER TABLE [dbo].[Originator] ADD  CONSTRAINT [DF_Originator_ModifiedTime]  DEFAULT (getdate()) FOR [ModifiedTime]
GO
ALTER TABLE [dbo].[Originator] ADD  CONSTRAINT [DF_Originator_CreatedBy]  DEFAULT (N'CATS Admin') FOR [CreatedBy]
GO
ALTER TABLE [dbo].[Originator] ADD  CONSTRAINT [DF_Originator_ModifiedBy]  DEFAULT (N'CATS Admin') FOR [ModifiedBy]
GO
ALTER TABLE [dbo].[Originator] ADD  CONSTRAINT [DF_Originator_IsDeleted]  DEFAULT ((0)) FOR [IsDeleted]
GO
ALTER TABLE [dbo].[Originator] ADD  CONSTRAINT [DF_Originator_IsEmailSent]  DEFAULT ((0)) FOR [IsEmailSent]
GO
ALTER TABLE [dbo].[Reviewer] ADD  CONSTRAINT [DF_Reviewer_IsRoundCompletedBySurrogate]  DEFAULT ((0)) FOR [IsRoundCompletedBySurrogate]
GO
ALTER TABLE [dbo].[Reviewer] ADD  CONSTRAINT [DF_Reviewer_CreatedTime]  DEFAULT (getdate()) FOR [CreatedTime]
GO
ALTER TABLE [dbo].[Reviewer] ADD  CONSTRAINT [DF_Reviewer_ModifiedTime]  DEFAULT (getdate()) FOR [ModifiedTime]
GO
ALTER TABLE [dbo].[Reviewer] ADD  CONSTRAINT [DF_Reviewer_CreatedBy]  DEFAULT (N'CATS Admin') FOR [CreatedBy]
GO
ALTER TABLE [dbo].[Reviewer] ADD  CONSTRAINT [DF_Reviewer_ModifiedBy]  DEFAULT (N'CATS Admin') FOR [ModifiedBy]
GO
ALTER TABLE [dbo].[Reviewer] ADD  CONSTRAINT [DF_Reviewer_IsDeleted]  DEFAULT ((0)) FOR [IsDeleted]
GO
ALTER TABLE [dbo].[Reviewer] ADD  CONSTRAINT [DF_Reviewer_IsEmailSent]  DEFAULT ((0)) FOR [IsEmailSent]
GO
ALTER TABLE [dbo].[Reviewer] ADD  CONSTRAINT [DF_Reviewer_IsCurrantRound]  DEFAULT ((0)) FOR [IsCurrentRound]
GO
ALTER TABLE [dbo].[ReviewRound] ADD  CONSTRAINT [DF_ReviewRound_IsCombinedRounds]  DEFAULT ((0)) FOR [IsCombinedRounds]
GO
ALTER TABLE [dbo].[ReviewRound] ADD  CONSTRAINT [DF_ReviewRound_CreatedBy]  DEFAULT (N'CATS Admin') FOR [CreatedBy]
GO
ALTER TABLE [dbo].[ReviewRound] ADD  CONSTRAINT [DF_ReviewRound_ModifiedBy]  DEFAULT (N'CATS Admin') FOR [ModifiedBy]
GO
ALTER TABLE [dbo].[ReviewRound] ADD  CONSTRAINT [DF_ReviewRound_CreatedTime]  DEFAULT (getdate()) FOR [CreatedTime]
GO
ALTER TABLE [dbo].[ReviewRound] ADD  CONSTRAINT [DF_ReviewRound_ModifiedTime]  DEFAULT (getdate()) FOR [ModifiedTime]
GO
ALTER TABLE [dbo].[ReviewRound] ADD  CONSTRAINT [DF_ReviewRound_IsDeleted]  DEFAULT ((0)) FOR [IsDeleted]
GO
ALTER TABLE [dbo].[Role] ADD  CONSTRAINT [DF_Roles_CreatedBy]  DEFAULT (N'CATS Admin') FOR [CreatedBy]
GO
ALTER TABLE [dbo].[Role] ADD  CONSTRAINT [DF_Roles_CreatedTime]  DEFAULT (getdate()) FOR [CreatedTime]
GO
ALTER TABLE [dbo].[Role] ADD  CONSTRAINT [DF_Roles_ModifiedTime]  DEFAULT (getdate()) FOR [ModifiedTime]
GO
ALTER TABLE [dbo].[Role] ADD  CONSTRAINT [DF_Role_IsDeleted]  DEFAULT ((0)) FOR [IsDeleted]
GO
ALTER TABLE [dbo].[Collaboration]  WITH CHECK ADD  CONSTRAINT [FK_Collaboration_Correspondence] FOREIGN KEY([CorrespondenceId])
REFERENCES [dbo].[Correspondence] ([Id])
GO
ALTER TABLE [dbo].[Collaboration] CHECK CONSTRAINT [FK_Collaboration_Correspondence]
GO
ALTER TABLE [dbo].[CorrespondenceCopiedArchived]  WITH CHECK ADD  CONSTRAINT [FK_CorrespondenceCopiedArchived_CorrespondenceCopiedArchived] FOREIGN KEY([CorrespondenceId])
REFERENCES [dbo].[Correspondence] ([Id])
GO
ALTER TABLE [dbo].[CorrespondenceCopiedArchived] CHECK CONSTRAINT [FK_CorrespondenceCopiedArchived_CorrespondenceCopiedArchived]
GO
ALTER TABLE [dbo].[CorrespondenceCopiedOffice]  WITH CHECK ADD  CONSTRAINT [FK_CorrespondenceCopiedOffice_Correspondence] FOREIGN KEY([CorrespondenceId])
REFERENCES [dbo].[Correspondence] ([Id])
GO
ALTER TABLE [dbo].[CorrespondenceCopiedOffice] CHECK CONSTRAINT [FK_CorrespondenceCopiedOffice_Correspondence]
GO
ALTER TABLE [dbo].[DLGroupMembers]  WITH CHECK ADD  CONSTRAINT [FK_DLGroupMembers_DLGroup] FOREIGN KEY([DLGroupId])
REFERENCES [dbo].[DLGroup] ([Id])
GO
ALTER TABLE [dbo].[DLGroupMembers] CHECK CONSTRAINT [FK_DLGroupMembers_DLGroup]
GO
ALTER TABLE [dbo].[Document]  WITH CHECK ADD  CONSTRAINT [FK_Document_Folder] FOREIGN KEY([FolderId])
REFERENCES [dbo].[Folder] ([Id])
GO
ALTER TABLE [dbo].[Document] CHECK CONSTRAINT [FK_Document_Folder]
GO
ALTER TABLE [dbo].[EmailController]  WITH CHECK ADD  CONSTRAINT [FK_EmailController_EmailTemplate] FOREIGN KEY([EmailTemplateId])
REFERENCES [dbo].[EmailTemplate] ([Id])
GO
ALTER TABLE [dbo].[EmailController] CHECK CONSTRAINT [FK_EmailController_EmailTemplate]
GO
ALTER TABLE [dbo].[FYIUser]  WITH CHECK ADD  CONSTRAINT [FK_FYIUser_Collaboration1] FOREIGN KEY([CollaborationId])
REFERENCES [dbo].[Collaboration] ([Id])
GO
ALTER TABLE [dbo].[FYIUser] CHECK CONSTRAINT [FK_FYIUser_Collaboration1]
GO
ALTER TABLE [dbo].[LeadOfficeMember]  WITH CHECK ADD  CONSTRAINT [FK_LeadOfficeMember_LeadOffice] FOREIGN KEY([LeadOfficeId])
REFERENCES [dbo].[LeadOffice] ([Id])
GO
ALTER TABLE [dbo].[LeadOfficeMember] CHECK CONSTRAINT [FK_LeadOfficeMember_LeadOffice]
GO
ALTER TABLE [dbo].[LeadOfficeMember]  WITH CHECK ADD  CONSTRAINT [FK_LeadOfficeMember_Role] FOREIGN KEY([RoleId])
REFERENCES [dbo].[Role] ([Id])
GO
ALTER TABLE [dbo].[LeadOfficeMember] CHECK CONSTRAINT [FK_LeadOfficeMember_Role]
GO
ALTER TABLE [dbo].[LeadOfficeOfficeManager]  WITH CHECK ADD  CONSTRAINT [FK_LeadOfficeOfficeManager_LeadOffice] FOREIGN KEY([LeadOfficeId])
REFERENCES [dbo].[LeadOffice] ([Id])
GO
ALTER TABLE [dbo].[LeadOfficeOfficeManager] CHECK CONSTRAINT [FK_LeadOfficeOfficeManager_LeadOffice]
GO
ALTER TABLE [dbo].[LeadOfficeOfficeManager]  WITH CHECK ADD  CONSTRAINT [FK_LeadOfficeOfficeManager_Role] FOREIGN KEY([RoleId])
REFERENCES [dbo].[Role] ([Id])
GO
ALTER TABLE [dbo].[LeadOfficeOfficeManager] CHECK CONSTRAINT [FK_LeadOfficeOfficeManager_Role]
GO
ALTER TABLE [dbo].[Originator]  WITH CHECK ADD  CONSTRAINT [FK_Originator_Collaboration] FOREIGN KEY([CollaborationId])
REFERENCES [dbo].[Collaboration] ([Id])
GO
ALTER TABLE [dbo].[Originator] CHECK CONSTRAINT [FK_Originator_Collaboration]
GO
ALTER TABLE [dbo].[Reviewer]  WITH CHECK ADD  CONSTRAINT [FK_Reviewer_Collaboration] FOREIGN KEY([CollaborationId])
REFERENCES [dbo].[Collaboration] ([Id])
GO
ALTER TABLE [dbo].[Reviewer] CHECK CONSTRAINT [FK_Reviewer_Collaboration]
GO

/*** INSERT Initial data ***/

SET IDENTITY_INSERT [dbo].[DLGroup] ON 
GO
INSERT [dbo].[DLGroup] ([Id], [Name], [Description], [ModifiedTime], [CreatedTime], [DeletedTime], [CreatedBy], [ModifiedBy], [DeletedBy], [IsDeleted]) VALUES (1, N'DL-BranchChiefs', NULL, CAST(N'2020-09-04T16:31:07.5800000' AS DateTime2), CAST(N'2020-09-04T16:31:07.5800000' AS DateTime2), NULL, N'CATS Admin', N'CATS Admin', NULL, 0)
GO
INSERT [dbo].[DLGroup] ([Id], [Name], [Description], [ModifiedTime], [CreatedTime], [DeletedTime], [CreatedBy], [ModifiedBy], [DeletedBy], [IsDeleted]) VALUES (2, N'DL-Communications', NULL, CAST(N'2020-09-04T16:31:07.5800000' AS DateTime2), CAST(N'2020-09-04T16:31:07.5800000' AS DateTime2), NULL, N'CATS Admin', N'CATS Admin', NULL, 0)
GO
INSERT [dbo].[DLGroup] ([Id], [Name], [Description], [ModifiedTime], [CreatedTime], [DeletedTime], [CreatedBy], [ModifiedBy], [DeletedBy], [IsDeleted]) VALUES (3, N'DL-DADS', NULL, CAST(N'2020-09-04T16:31:07.5800000' AS DateTime2), CAST(N'2020-09-04T16:31:07.5800000' AS DateTime2), NULL, N'CATS Admin', N'CATS Admin', NULL, 0)
GO
INSERT [dbo].[DLGroup] ([Id], [Name], [Description], [ModifiedTime], [CreatedTime], [DeletedTime], [CreatedBy], [ModifiedBy], [DeletedBy], [IsDeleted]) VALUES (4, N'DL-FinalPackaging', NULL, CAST(N'2020-09-04T16:31:07.5800000' AS DateTime2), CAST(N'2020-09-04T16:31:07.5800000' AS DateTime2), NULL, N'CATS Admin', N'CATS Admin', NULL, 0)
GO
INSERT [dbo].[DLGroup] ([Id], [Name], [Description], [ModifiedTime], [CreatedTime], [DeletedTime], [CreatedBy], [ModifiedBy], [DeletedBy], [IsDeleted]) VALUES (5, N'DL-GrayBuildingGeneralCounsel', NULL, CAST(N'2020-09-04T16:31:07.5800000' AS DateTime2), CAST(N'2020-09-04T16:31:07.5800000' AS DateTime2), NULL, N'CATS Admin', N'CATS Admin', NULL, 0)
GO
INSERT [dbo].[DLGroup] ([Id], [Name], [Description], [ModifiedTime], [CreatedTime], [DeletedTime], [CreatedBy], [ModifiedBy], [DeletedBy], [IsDeleted]) VALUES (6, N'DL-RedBuildingGeneralCounsel', NULL, CAST(N'2020-09-04T16:31:07.5800000' AS DateTime2), CAST(N'2020-09-04T16:31:07.5800000' AS DateTime2), NULL, N'CATS Admin', N'CATS Admin', NULL, 0)
GO
INSERT [dbo].[DLGroup] ([Id], [Name], [Description], [ModifiedTime], [CreatedTime], [DeletedTime], [CreatedBy], [ModifiedBy], [DeletedBy], [IsDeleted]) VALUES (7, N'DL-PADS', NULL, CAST(N'2020-09-04T16:31:07.5800000' AS DateTime2), CAST(N'2020-09-04T16:31:07.5800000' AS DateTime2), NULL, N'CATS Admin', N'CATS Admin', NULL, 0)
GO
INSERT [dbo].[DLGroup] ([Id], [Name], [Description], [ModifiedTime], [CreatedTime], [DeletedTime], [CreatedBy], [ModifiedBy], [DeletedBy], [IsDeleted]) VALUES (8, N'DL-LegAffairs', NULL, CAST(N'2020-09-04T16:31:07.5800000' AS DateTime2), CAST(N'2020-09-04T16:31:07.5800000' AS DateTime2), NULL, N'CATS Admin', N'CATS Admin', NULL, 0)
GO
INSERT [dbo].[DLGroup] ([Id], [Name], [Description], [ModifiedTime], [CreatedTime], [DeletedTime], [CreatedBy], [ModifiedBy], [DeletedBy], [IsDeleted]) VALUES (9, N'DL-TEST-GROUP', NULL, CAST(N'2020-09-29T11:34:59.1200000' AS DateTime2), CAST(N'2020-09-29T11:34:59.1200000' AS DateTime2), NULL, N'CATS Admin', N'CATS Admin', NULL, 0)
GO
SET IDENTITY_INSERT [dbo].[DLGroup] OFF
GO
INSERT [dbo].[DocumentType] ([Id], [Name], [Description], [CreatedTime], [ModifiedTime], [DeletedTime], [DeletedBy], [CreatedBy], [ModifiedBy], [IsDeleted]) VALUES (1, N'Reference', N'Reference Document', CAST(N'2020-06-05T00:00:00.0000000' AS DateTime2), CAST(N'2020-06-05T00:00:00.0000000' AS DateTime2), CAST(N'2020-06-05T00:00:00.0000000' AS DateTime2), NULL, NULL, NULL, NULL)
GO
INSERT [dbo].[DocumentType] ([Id], [Name], [Description], [CreatedTime], [ModifiedTime], [DeletedTime], [DeletedBy], [CreatedBy], [ModifiedBy], [IsDeleted]) VALUES (2, N'Review', N'Review Document', CAST(N'2020-06-05T00:00:00.0000000' AS DateTime2), CAST(N'2020-06-05T00:00:00.0000000' AS DateTime2), CAST(N'2020-06-05T00:00:00.0000000' AS DateTime2), NULL, NULL, NULL, NULL)
GO
INSERT [dbo].[DocumentType] ([Id], [Name], [Description], [CreatedTime], [ModifiedTime], [DeletedTime], [DeletedBy], [CreatedBy], [ModifiedBy], [IsDeleted]) VALUES (3, N'Final Document', N'Final Document', CAST(N'2020-06-05T00:00:00.0000000' AS DateTime2), CAST(N'2020-06-05T00:00:00.0000000' AS DateTime2), CAST(N'2020-06-05T00:00:00.0000000' AS DateTime2), NULL, NULL, NULL, NULL)
GO
SET IDENTITY_INSERT [dbo].[LeadOffice] ON 
GO
INSERT [dbo].[LeadOffice] ([Id], [Name], [Description], [IsDeleted], [CreatedBy], [ModifiedBy], [DeletedBy], [CreatedTime], [ModifiedTime], [DeletedTime], [IsHidden]) VALUES (1, N'BRD', N'BRD - Office Assistant Director Budget Review', 0, N'CATS Admin', N'CATS Admin', N'CATS Admin', CAST(N'2020-06-23T10:20:24.9966667' AS DateTime2), CAST(N'2020-06-23T10:20:24.9966667' AS DateTime2), NULL, 0)
GO
INSERT [dbo].[LeadOffice] ([Id], [Name], [Description], [IsDeleted], [CreatedBy], [ModifiedBy], [DeletedBy], [CreatedTime], [ModifiedTime], [DeletedTime], [IsHidden]) VALUES (2, N'EGOV', N'EGOV - E Government and Information Technology', 0, N'CATS Admin', N'CATS Admin', N'CATS Admin', CAST(N'2020-06-23T10:20:24.9966667' AS DateTime2), CAST(N'2020-06-23T10:20:24.9966667' AS DateTime2), NULL, 0)
GO
INSERT [dbo].[LeadOffice] ([Id], [Name], [Description], [IsDeleted], [CreatedBy], [ModifiedBy], [DeletedBy], [CreatedTime], [ModifiedTime], [DeletedTime], [IsHidden]) VALUES (3, N'EIML', N'EIML - Education, Income Maintenance and Labor Programs', 0, N'CATS Admin', N'CATS Admin', N'CATS Admin', CAST(N'2020-06-23T10:20:24.9966667' AS DateTime2), CAST(N'2020-06-23T10:20:24.9966667' AS DateTime2), NULL, 0)
GO
INSERT [dbo].[LeadOffice] ([Id], [Name], [Description], [IsDeleted], [CreatedBy], [ModifiedBy], [DeletedBy], [CreatedTime], [ModifiedTime], [DeletedTime], [IsHidden]) VALUES (4, N'EP', N'EP - Economic Policy', 0, N'CATS Admin', N'CATS Admin', N'CATS Admin', CAST(N'2020-06-23T10:20:24.9966667' AS DateTime2), CAST(N'2020-06-23T10:20:24.9966667' AS DateTime2), NULL, 0)
GO
INSERT [dbo].[LeadOffice] ([Id], [Name], [Description], [IsDeleted], [CreatedBy], [ModifiedBy], [DeletedBy], [CreatedTime], [ModifiedTime], [DeletedTime], [IsHidden]) VALUES (5, N'ESW', N'ESW - Energy Branch', 0, N'CATS Admin', N'CATS Admin', N'CATS Admin', CAST(N'2020-06-23T10:20:24.9966667' AS DateTime2), CAST(N'2020-06-23T10:20:24.9966667' AS DateTime2), NULL, 0)
GO
INSERT [dbo].[LeadOffice] ([Id], [Name], [Description], [IsDeleted], [CreatedBy], [ModifiedBy], [DeletedBy], [CreatedTime], [ModifiedTime], [DeletedTime], [IsHidden]) VALUES (6, N'HEALTH', N'HEALTH - Health Programs', 0, N'CATS Admin', N'CATS Admin', N'CATS Admin', CAST(N'2020-06-23T10:20:24.9966667' AS DateTime2), CAST(N'2020-06-23T10:20:24.9966667' AS DateTime2), NULL, 0)
GO
INSERT [dbo].[LeadOffice] ([Id], [Name], [Description], [IsDeleted], [CreatedBy], [ModifiedBy], [DeletedBy], [CreatedTime], [ModifiedTime], [DeletedTime], [IsHidden]) VALUES (7, N'HTC', N'HTC - Housing / Treasury / Commerce Division ', 0, N'CATS Admin', N'CATS Admin', N'CATS Admin', CAST(N'2020-06-23T10:20:24.9966667' AS DateTime2), CAST(N'2020-06-23T10:20:24.9966667' AS DateTime2), NULL, 0)
GO
INSERT [dbo].[LeadOffice] ([Id], [Name], [Description], [IsDeleted], [CreatedBy], [ModifiedBy], [DeletedBy], [CreatedTime], [ModifiedTime], [DeletedTime], [IsHidden]) VALUES (8, N'IAD', N'IAD - International Affairs Division ', 0, N'CATS Admin', N'CATS Admin', N'CATS Admin', CAST(N'2020-06-23T10:20:24.9966667' AS DateTime2), CAST(N'2020-06-23T10:20:24.9966667' AS DateTime2), NULL, 0)
GO
INSERT [dbo].[LeadOffice] ([Id], [Name], [Description], [IsDeleted], [CreatedBy], [ModifiedBy], [DeletedBy], [CreatedTime], [ModifiedTime], [DeletedTime], [IsHidden]) VALUES (9, N'IPEC', N'IPEC - Intellectual Property Enforcement Coordinator ', 0, N'CATS Admin', N'CATS Admin', N'CATS Admin', CAST(N'2020-06-23T10:20:24.9966667' AS DateTime2), CAST(N'2020-06-23T10:20:24.9966667' AS DateTime2), NULL, 0)
GO
INSERT [dbo].[LeadOffice] ([Id], [Name], [Description], [IsDeleted], [CreatedBy], [ModifiedBy], [DeletedBy], [CreatedTime], [ModifiedTime], [DeletedTime], [IsHidden]) VALUES (10, N'MOD', N'MOD - Management and Operations Division ', 0, N'CATS Admin', N'CATS Admin', N'CATS Admin', CAST(N'2020-06-23T10:20:24.9966667' AS DateTime2), CAST(N'2020-06-23T10:20:24.9966667' AS DateTime2), NULL, 0)
GO
INSERT [dbo].[LeadOffice] ([Id], [Name], [Description], [IsDeleted], [CreatedBy], [ModifiedBy], [DeletedBy], [CreatedTime], [ModifiedTime], [DeletedTime], [IsHidden]) VALUES (11, N'NRD', N'NRD - Natural Resources Division', 0, N'CATS Admin', N'CATS Admin', N'CATS Admin', CAST(N'2020-06-23T10:20:24.9966667' AS DateTime2), CAST(N'2020-06-23T10:20:24.9966667' AS DateTime2), NULL, 0)
GO
INSERT [dbo].[LeadOffice] ([Id], [Name], [Description], [IsDeleted], [CreatedBy], [ModifiedBy], [DeletedBy], [CreatedTime], [ModifiedTime], [DeletedTime], [IsHidden]) VALUES (12, N'OFFM', N'OFFM - Office of Federal Financial Management ', 0, N'CATS Admin', N'CATS Admin', N'CATS Admin', CAST(N'2020-06-23T10:20:24.9966667' AS DateTime2), CAST(N'2020-06-23T10:20:24.9966667' AS DateTime2), NULL, 0)
GO
INSERT [dbo].[LeadOffice] ([Id], [Name], [Description], [IsDeleted], [CreatedBy], [ModifiedBy], [DeletedBy], [CreatedTime], [ModifiedTime], [DeletedTime], [IsHidden]) VALUES (13, N'OFPP', N'OFPP - Office of Federal Procurement Policy ', 0, N'CATS Admin', N'CATS Admin', N'CATS Admin', CAST(N'2020-06-23T10:20:24.9966667' AS DateTime2), CAST(N'2020-06-23T10:20:24.9966667' AS DateTime2), NULL, 0)
GO
INSERT [dbo].[LeadOffice] ([Id], [Name], [Description], [IsDeleted], [CreatedBy], [ModifiedBy], [DeletedBy], [CreatedTime], [ModifiedTime], [DeletedTime], [IsHidden]) VALUES (14, N'OIRA', N'OIRA - Office of Information and Regulatory Affairs ', 0, N'CATS Admin', N'CATS Admin', N'CATS Admin', CAST(N'2020-06-23T10:20:24.9966667' AS DateTime2), CAST(N'2020-06-23T10:20:24.9966667' AS DateTime2), NULL, 0)
GO
INSERT [dbo].[LeadOffice] ([Id], [Name], [Description], [IsDeleted], [CreatedBy], [ModifiedBy], [DeletedBy], [CreatedTime], [ModifiedTime], [DeletedTime], [IsHidden]) VALUES (15, N'PPM', N'PPM - Performance and Personnel Management Division ', 0, N'CATS Admin', N'CATS Admin', N'CATS Admin', CAST(N'2020-06-23T10:20:24.9966667' AS DateTime2), CAST(N'2020-06-23T10:20:24.9966667' AS DateTime2), NULL, 0)
GO
INSERT [dbo].[LeadOffice] ([Id], [Name], [Description], [IsDeleted], [CreatedBy], [ModifiedBy], [DeletedBy], [CreatedTime], [ModifiedTime], [DeletedTime], [IsHidden]) VALUES (16, N'THJS', N'THJS - Transportation / Homeland Security / Justice Division ', 0, N'CATS Admin', N'CATS Admin', N'CATS Admin', CAST(N'2020-06-23T10:20:24.9966667' AS DateTime2), CAST(N'2020-06-23T10:20:24.9966667' AS DateTime2), NULL, 0)
GO
INSERT [dbo].[LeadOffice] ([Id], [Name], [Description], [IsDeleted], [CreatedBy], [ModifiedBy], [DeletedBy], [CreatedTime], [ModifiedTime], [DeletedTime], [IsHidden]) VALUES (17, N'USDS', N'USDS - ITOR / USDS', 0, N'CATS Admin', N'CATS Admin', N'CATS Admin', CAST(N'2020-06-23T10:20:24.9966667' AS DateTime2), CAST(N'2020-06-23T10:20:24.9966667' AS DateTime2), NULL, 0)
GO
INSERT [dbo].[LeadOffice] ([Id], [Name], [Description], [IsDeleted], [CreatedBy], [ModifiedBy], [DeletedBy], [CreatedTime], [ModifiedTime], [DeletedTime], [IsHidden]) VALUES (18, N'GC', N'GC - Office of General Counsel ', 0, N'CATS Admin', N'CATS Admin', N'CATS Admin', CAST(N'2020-06-23T10:20:24.9966667' AS DateTime2), CAST(N'2020-06-23T10:20:24.9966667' AS DateTime2), NULL, 0)
GO
INSERT [dbo].[LeadOffice] ([Id], [Name], [Description], [IsDeleted], [CreatedBy], [ModifiedBy], [DeletedBy], [CreatedTime], [ModifiedTime], [DeletedTime], [IsHidden]) VALUES (19, N'NSD', N'NSD - National Security Division ', 0, N'CATS Admin', N'CATS Admin', N'CATS Admin', CAST(N'2020-06-23T10:20:24.9966667' AS DateTime2), CAST(N'2020-06-23T10:20:24.9966667' AS DateTime2), NULL, 0)
GO
INSERT [dbo].[LeadOffice] ([Id], [Name], [Description], [IsDeleted], [CreatedBy], [ModifiedBy], [DeletedBy], [CreatedTime], [ModifiedTime], [DeletedTime], [IsHidden]) VALUES (20, N'LA', N'LA - Office of Legislative Affairs', 0, N'CATS Admin', N'CATS Admin', N'CATS Admin', CAST(N'2020-06-23T10:20:24.9966667' AS DateTime2), CAST(N'2020-06-23T10:20:24.9966667' AS DateTime2), NULL, 0)
GO
INSERT [dbo].[LeadOffice] ([Id], [Name], [Description], [IsDeleted], [CreatedBy], [ModifiedBy], [DeletedBy], [CreatedTime], [ModifiedTime], [DeletedTime], [IsHidden]) VALUES (21, N'DO', N'DO - Office of the Director', 0, N'CATS Admin', N'CATS Admin', N'CATS Admin', CAST(N'2020-06-23T10:20:24.9966667' AS DateTime2), CAST(N'2020-06-23T10:20:24.9966667' AS DateTime2), NULL, 0)
GO
INSERT [dbo].[LeadOffice] ([Id], [Name], [Description], [IsDeleted], [CreatedBy], [ModifiedBy], [DeletedBy], [CreatedTime], [ModifiedTime], [DeletedTime], [IsHidden]) VALUES (22, N'COMMS', N'COMMS - Strategic Planning and Communications', 0, N'CATS Admin', N'CATS Admin', N'CATS Admin', CAST(N'2020-06-23T10:52:43.0800000' AS DateTime2), CAST(N'2020-06-23T10:52:43.0800000' AS DateTime2), NULL, 0)
GO
INSERT [dbo].[LeadOffice] ([Id], [Name], [Description], [IsDeleted], [CreatedBy], [ModifiedBy], [DeletedBy], [CreatedTime], [ModifiedTime], [DeletedTime], [IsHidden]) VALUES (23, N'LRD', N'LRD - Associate Director for Legislative Reference Division ', 0, N'CATS Admin', N'CATS Admin', N'CATS Admin', CAST(N'2020-06-23T10:52:59.1300000' AS DateTime2), CAST(N'2020-06-23T10:52:59.1300000' AS DateTime2), NULL, 0)
GO
INSERT [dbo].[LeadOffice] ([Id], [Name], [Description], [IsDeleted], [CreatedBy], [ModifiedBy], [DeletedBy], [CreatedTime], [ModifiedTime], [DeletedTime], [IsHidden]) VALUES (24, N'CORRESPONDENCE', N'CORRESPONDENCE UNIT TEAM', 0, N'CATS Admin', N'CATS Admin', N'CATS Admin', CAST(N'2020-06-23T11:03:41.0033333' AS DateTime2), CAST(N'2020-06-23T11:03:41.0033333' AS DateTime2), NULL, 0)
GO
INSERT [dbo].[LeadOffice] ([Id], [Name], [Description], [IsDeleted], [CreatedBy], [ModifiedBy], [DeletedBy], [CreatedTime], [ModifiedTime], [DeletedTime], [IsHidden]) VALUES (25, N'EXTOFFICE', N'External Originators', 0, N'CATS Admin', N'CATS Admin', N'CATS Admin', CAST(N'2020-10-07T13:29:56.6666667' AS DateTime2), CAST(N'2020-10-07T13:29:56.6666667' AS DateTime2), NULL, 1)
GO
INSERT [dbo].[LeadOffice] ([Id], [Name], [Description], [IsDeleted], [CreatedBy], [ModifiedBy], [DeletedBy], [CreatedTime], [ModifiedTime], [DeletedTime], [IsHidden]) VALUES (26, N'OTHER', N'OTHER', 0, N'CATS Admin', N'CATS Admin', N'CATS Admin', CAST(N'2021-02-10T12:41:21.3833333' AS DateTime2), CAST(N'2021-02-10T12:41:21.3833333' AS DateTime2), NULL, 0)
GO
INSERT [dbo].[LeadOffice] ([Id], [Name], [Description], [IsDeleted], [CreatedBy], [ModifiedBy], [DeletedBy], [CreatedTime], [ModifiedTime], [DeletedTime], [IsHidden]) VALUES (27, N'CATS SUPPORT', N'CATS Support Team', 0, N'CATS Admin', N'CATS Admin', N'CATS Admin', CAST(N'2021-02-23T15:42:27.9466667' AS DateTime2), CAST(N'2021-02-23T15:42:27.9466667' AS DateTime2), NULL, 1)
GO
SET IDENTITY_INSERT [dbo].[LeadOffice] OFF
GO

SET IDENTITY_INSERT [dbo].[HelpDocument] ON 
GO
INSERT [dbo].[HelpDocument] ([Id], [Title], [Url], [CreatedTime], [ModifiedTime], [DeletedTime], [CreatedBy], [ModifiedBy], [DeletedBy], [IsDeleted], [IsExternal], [IsDevider], [Icon]) VALUES (1, N'Introduction to CATS', N'Introduction to CATS 8-30-18.pdf', CAST(N'2020-12-02T14:03:45.2300000' AS DateTime2), CAST(N'2020-12-02T14:03:45.2300000' AS DateTime2), NULL, N'Jean Pita', N'Jean Pita', NULL, 0, 0, 0, N'text_snippet')
GO
INSERT [dbo].[HelpDocument] ([Id], [Title], [Url], [CreatedTime], [ModifiedTime], [DeletedTime], [CreatedBy], [ModifiedBy], [DeletedBy], [IsDeleted], [IsExternal], [IsDevider], [Icon]) VALUES (2, N'How to Navigate the Correspondence Dashboard', N'How to Navigate the Correspondence Dashboard 8-30-18.pdf', CAST(N'2020-12-02T14:03:54.6333333' AS DateTime2), CAST(N'2020-12-02T14:03:54.6333333' AS DateTime2), NULL, N'Jean Pita', N'Jean Pita', NULL, 0, 0, 0, N'text_snippet')
GO
INSERT [dbo].[HelpDocument] ([Id], [Title], [Url], [CreatedTime], [ModifiedTime], [DeletedTime], [CreatedBy], [ModifiedBy], [DeletedBy], [IsDeleted], [IsExternal], [IsDevider], [Icon]) VALUES (3, N'How to Navigate the Originator Dashboard', N'How to Navigate the Originator Dashboard 11-8-18.pdf', CAST(N'2020-12-02T14:04:08.9200000' AS DateTime2), CAST(N'2020-12-02T14:04:08.9200000' AS DateTime2), NULL, N'Jean Pita', N'Jean Pita', NULL, 0, 0, 0, N'text_snippet')
GO
INSERT [dbo].[HelpDocument] ([Id], [Title], [Url], [CreatedTime], [ModifiedTime], [DeletedTime], [CreatedBy], [ModifiedBy], [DeletedBy], [IsDeleted], [IsExternal], [IsDevider], [Icon]) VALUES (4, N'How to Navigate the Reviewer Dashboard', N'How to Navigate the Reviewer Dashboard 8-30-18.pdf', CAST(N'2020-12-02T14:04:30.8366667' AS DateTime2), CAST(N'2020-12-02T14:04:30.8366667' AS DateTime2), NULL, N'Jean Pita', N'Jean Pita', NULL, 0, 0, 0, N'text_snippet')
GO
INSERT [dbo].[HelpDocument] ([Id], [Title], [Url], [CreatedTime], [ModifiedTime], [DeletedTime], [CreatedBy], [ModifiedBy], [DeletedBy], [IsDeleted], [IsExternal], [IsDevider], [Icon]) VALUES (5, N'Divider', NULL, CAST(N'2020-12-02T14:05:14.8366667' AS DateTime2), CAST(N'2020-12-02T14:05:14.8366667' AS DateTime2), NULL, N'Jean Pita', N'Jean Pita', NULL, 0, 0, 1, N'text_snippet')
GO
INSERT [dbo].[HelpDocument] ([Id], [Title], [Url], [CreatedTime], [ModifiedTime], [DeletedTime], [CreatedBy], [ModifiedBy], [DeletedBy], [IsDeleted], [IsExternal], [IsDevider], [Icon]) VALUES (6, N'How to Launch a New Collaboration', N'How to Launch a New Collaboration 8-28-18 (1).pdf', CAST(N'2020-12-02T14:06:39.2866667' AS DateTime2), CAST(N'2020-12-02T14:06:39.2866667' AS DateTime2), NULL, N'Jean Pita', N'Jean Pita', NULL, 0, 0, 0, N'text_snippet')
GO
INSERT [dbo].[HelpDocument] ([Id], [Title], [Url], [CreatedTime], [ModifiedTime], [DeletedTime], [CreatedBy], [ModifiedBy], [DeletedBy], [IsDeleted], [IsExternal], [IsDevider], [Icon]) VALUES (7, N'How to Manage a Review Round', N'How to Manage a Review Round 8-28-18.pdf', CAST(N'2020-12-02T14:06:53.9733333' AS DateTime2), CAST(N'2020-12-02T14:06:53.9733333' AS DateTime2), NULL, N'Jean Pita', N'Jean Pita', NULL, 0, 0, 0, N'text_snippet')
GO
INSERT [dbo].[HelpDocument] ([Id], [Title], [Url], [CreatedTime], [ModifiedTime], [DeletedTime], [CreatedBy], [ModifiedBy], [DeletedBy], [IsDeleted], [IsExternal], [IsDevider], [Icon]) VALUES (8, N'How to Move Packages to the Next Round', N'How to Move Packages to the Next Round 8-28-18.pdf', CAST(N'2020-12-02T14:07:01.8233333' AS DateTime2), CAST(N'2020-12-02T14:07:01.8233333' AS DateTime2), NULL, N'Jean Pita', N'Jean Pita', NULL, 0, 0, 0, N'text_snippet')
GO
INSERT [dbo].[HelpDocument] ([Id], [Title], [Url], [CreatedTime], [ModifiedTime], [DeletedTime], [CreatedBy], [ModifiedBy], [DeletedBy], [IsDeleted], [IsExternal], [IsDevider], [Icon]) VALUES (9, N'How to Review a Document', N'How to Review a Document 8-30-18.pdf', CAST(N'2020-12-02T14:07:15.2100000' AS DateTime2), CAST(N'2020-12-02T14:07:15.2100000' AS DateTime2), NULL, N'Jean Pita', N'Jean Pita', NULL, 0, 0, 0, N'text_snippet')
GO
INSERT [dbo].[HelpDocument] ([Id], [Title], [Url], [CreatedTime], [ModifiedTime], [DeletedTime], [CreatedBy], [ModifiedBy], [DeletedBy], [IsDeleted], [IsExternal], [IsDevider], [Icon]) VALUES (10, N'How to Assign Surrogates', N'How to Assign Surrogates 8-28-18.pdf', CAST(N'2020-12-02T14:07:18.6066667' AS DateTime2), CAST(N'2020-12-02T14:07:18.6066667' AS DateTime2), NULL, N'Jean Pita', N'Jean Pita', NULL, 0, 0, 0, N'text_snippet')
GO
INSERT [dbo].[HelpDocument] ([Id], [Title], [Url], [CreatedTime], [ModifiedTime], [DeletedTime], [CreatedBy], [ModifiedBy], [DeletedBy], [IsDeleted], [IsExternal], [IsDevider], [Icon]) VALUES (11, N'Divider', NULL, CAST(N'2020-12-02T14:07:24.8600000' AS DateTime2), CAST(N'2020-12-02T14:07:24.8600000' AS DateTime2), NULL, N'Jean Pita', N'Jean Pita', NULL, 0, 0, 1, N'text_snippet')
GO
INSERT [dbo].[HelpDocument] ([Id], [Title], [Url], [CreatedTime], [ModifiedTime], [DeletedTime], [CreatedBy], [ModifiedBy], [DeletedBy], [IsDeleted], [IsExternal], [IsDevider], [Icon]) VALUES (12, N'Correspondence and Clearance Business Process Overview', N'Correspondence and Clearance Business Process Overview Print V 10-3-19.pdf', CAST(N'2020-12-02T14:07:41.7500000' AS DateTime2), CAST(N'2020-12-02T14:07:41.7500000' AS DateTime2), NULL, N'Jean Pita', N'Jean Pita', NULL, 0, 0, 0, N'text_snippet')
GO
INSERT [dbo].[HelpDocument] ([Id], [Title], [Url], [CreatedTime], [ModifiedTime], [DeletedTime], [CreatedBy], [ModifiedBy], [DeletedBy], [IsDeleted], [IsExternal], [IsDevider], [Icon]) VALUES (13, N'Roles and Responsibilities', N'Roles and Responsibilities.pdf', CAST(N'2020-12-02T14:07:50.8700000' AS DateTime2), CAST(N'2020-12-02T14:07:50.8700000' AS DateTime2), NULL, N'Jean Pita', N'Jean Pita', NULL, 0, 0, 0, N'text_snippet')
GO
INSERT [dbo].[HelpDocument] ([Id], [Title], [Url], [CreatedTime], [ModifiedTime], [DeletedTime], [CreatedBy], [ModifiedBy], [DeletedBy], [IsDeleted], [IsExternal], [IsDevider], [Icon]) VALUES (14, N'Business Process Rules', N'Business Process Rules.pdf Print V10-3-19.pdf', CAST(N'2020-12-02T14:08:09.9900000' AS DateTime2), CAST(N'2020-12-02T14:08:09.9900000' AS DateTime2), NULL, N'Jean Pita', N'Jean Pita', NULL, 0, 0, 0, N'text_snippet')
GO
INSERT [dbo].[HelpDocument] ([Id], [Title], [Url], [CreatedTime], [ModifiedTime], [DeletedTime], [CreatedBy], [ModifiedBy], [DeletedBy], [IsDeleted], [IsExternal], [IsDevider], [Icon]) VALUES (15, N'Divider', NULL, CAST(N'2020-12-02T14:08:15.4733333' AS DateTime2), CAST(N'2020-12-02T14:08:15.4733333' AS DateTime2), NULL, N'Jean Pita', N'Jean Pita', NULL, 0, 0, 1, N'text_snippet')
GO
INSERT [dbo].[HelpDocument] ([Id], [Title], [Url], [CreatedTime], [ModifiedTime], [DeletedTime], [CreatedBy], [ModifiedBy], [DeletedBy], [IsDeleted], [IsExternal], [IsDevider], [Icon]) VALUES (16, N'Correspondence Letter Review Process', N'Correspondence Letter Review Process.pdf Print V 10-3-19.pdf', CAST(N'2020-12-02T14:08:33.5733333' AS DateTime2), CAST(N'2020-12-02T14:08:33.5733333' AS DateTime2), NULL, N'Jean Pita', N'Jean Pita', NULL, 0, 0, 0, N'text_snippet')
GO
INSERT [dbo].[HelpDocument] ([Id], [Title], [Url], [CreatedTime], [ModifiedTime], [DeletedTime], [CreatedBy], [ModifiedBy], [DeletedBy], [IsDeleted], [IsExternal], [IsDevider], [Icon]) VALUES (17, N'720 Letter Review Process', N'720 Letter Review Process.pdf', CAST(N'2020-12-02T14:08:42.7033333' AS DateTime2), CAST(N'2020-12-02T14:08:42.7033333' AS DateTime2), NULL, N'Jean Pita', N'Jean Pita', NULL, 0, 0, 0, N'text_snippet')
GO
INSERT [dbo].[HelpDocument] ([Id], [Title], [Url], [CreatedTime], [ModifiedTime], [DeletedTime], [CreatedBy], [ModifiedBy], [DeletedBy], [IsDeleted], [IsExternal], [IsDevider], [Icon]) VALUES (18, N'Memoranda, Guidance and Reports Review Process', N'Memoranda Guidance Reports Review Process.pdf', CAST(N'2020-12-02T14:08:53.8133333' AS DateTime2), CAST(N'2020-12-02T14:08:53.8133333' AS DateTime2), NULL, N'Jean Pita', N'Jean Pita', NULL, 0, 0, 0, N'text_snippet')
GO
INSERT [dbo].[HelpDocument] ([Id], [Title], [Url], [CreatedTime], [ModifiedTime], [DeletedTime], [CreatedBy], [ModifiedBy], [DeletedBy], [IsDeleted], [IsExternal], [IsDevider], [Icon]) VALUES (19, N'Federal Register Notices Review Process', N'Federal Register Notices Review Process.pdf', CAST(N'2020-12-02T14:09:01.4733333' AS DateTime2), CAST(N'2020-12-02T14:09:01.4733333' AS DateTime2), NULL, N'Jean Pita', N'Jean Pita', NULL, 0, 0, 0, N'text_snippet')
GO
INSERT [dbo].[HelpDocument] ([Id], [Title], [Url], [CreatedTime], [ModifiedTime], [DeletedTime], [CreatedBy], [ModifiedBy], [DeletedBy], [IsDeleted], [IsExternal], [IsDevider], [Icon]) VALUES (20, N'ADA Reports Review Process', N'ADA Reports Review Process.pdf', CAST(N'2020-12-02T14:09:05.9400000' AS DateTime2), CAST(N'2020-12-02T14:09:05.9400000' AS DateTime2), NULL, N'Jean Pita', N'Jean Pita', NULL, 0, 0, 0, N'text_snippet')
GO
INSERT [dbo].[HelpDocument] ([Id], [Title], [Url], [CreatedTime], [ModifiedTime], [DeletedTime], [CreatedBy], [ModifiedBy], [DeletedBy], [IsDeleted], [IsExternal], [IsDevider], [Icon]) VALUES (21, N'Divider', NULL, CAST(N'2020-12-02T14:09:17.0533333' AS DateTime2), CAST(N'2020-12-02T14:09:17.0533333' AS DateTime2), NULL, N'Jean Pita', N'Jean Pita', NULL, 0, 0, 1, N'text_snippet')
GO
INSERT [dbo].[HelpDocument] ([Id], [Title], [Url], [CreatedTime], [ModifiedTime], [DeletedTime], [CreatedBy], [ModifiedBy], [DeletedBy], [IsDeleted], [IsExternal], [IsDevider], [Icon]) VALUES (22, N'OMB Executive Secretary Resources', N'https://community.max.gov/pages/viewpage.action?spaceKey=OMB&title=OMB+Executive+Secretary+Resources', CAST(N'2020-12-02T14:09:25.4633333' AS DateTime2), CAST(N'2020-12-02T14:09:25.4633333' AS DateTime2), NULL, N'Jean Pita', N'Jean Pita', NULL, 0, 1, 0, N'forward')
GO
INSERT [dbo].[HelpDocument] ([Id], [Title], [Url], [CreatedTime], [ModifiedTime], [DeletedTime], [CreatedBy], [ModifiedBy], [DeletedBy], [IsDeleted], [IsExternal], [IsDevider], [Icon]) VALUES (23, N'Email CATS System Operator for assistance', N'mailto:DL.MODWebAppSupport@whmo.mil?Subject=Please assist with my request describe below...', CAST(N'2020-12-02T14:09:32.2000000' AS DateTime2), CAST(N'2020-12-02T14:09:32.2000000' AS DateTime2), NULL, N'Jean Pita', N'Jean Pita', NULL, 0, 1, 0, N'forward_to_in')
GO
SET IDENTITY_INSERT [dbo].[HelpDocument] OFF
GO
SET IDENTITY_INSERT [dbo].[LetterType] ON 
GO
INSERT [dbo].[LetterType] ([Id], [Name], [Description], [IsDeleted], [CreatedBy], [ModifiedBy], [DeletedBy], [CreatedTime], [DeletedTime], [ModifiedTime], [HtmlContent]) VALUES (1, N'720 Letter', NULL, 0, N'CATS Admin', N'CATS Admin', N'CATS Admin', CAST(N'2020-06-23T18:44:12.2466667' AS DateTime2), NULL, CAST(N'2020-06-23T18:44:12.2466667' AS DateTime2), N'<div class="col-xl-12" id="ncwc-modal-lw-rrv-DivID4">
   <div class="col-xl-12" style="margin-bottom&#58;20px;"> 
      <span>Based on your document type ( 
         <span style="color&#58;#0c5460;font-weight&#58;normal;">720 Letter</span> ), your document review process will go through rounds of reviews in the following order&#58;</span> </div><div style="margin-bottom&#58;20px;max-width&#58;90%;"><ol><li style="font-weight&#58;bold;border-bottom-width&#58;1px;border-bottom-style&#58;solid;">RED BUILDING GENERAL COUNSEL (<span style="color&#58;#0c5460;font-weight&#58;normal;"> 3 Business Days </span>)</li><li style="font-weight&#58;bold;border-bottom-width&#58;1px;border-bottom-style&#58;solid;">PAD/POLICY OFFICIALS (<span style="color&#58;#0c5460;font-weight&#58;normal;"> 3 Business Days </span>)</li><li style="font-weight&#58;bold;border-bottom-width&#58;1px;border-bottom-style&#58;solid;">COMMUNICATIONS &amp; LEGISLATIVE AFFAIRS (<span style="color&#58;#0c5460;font-weight&#58;normal;"> 3 Business Days </span>)</li><li style="font-weight&#58;bold;border-bottom-width&#58;1px;border-bottom-style&#58;solid;">GRAY BUILDING GENERAL COUNSEL (<span style="color&#58;#0c5460;font-weight&#58;normal;"> 3 Business Days </span>)</li><li style="font-weight&#58;bold;border-bottom-width&#58;1px;border-bottom-style&#58;solid;">FINAL PACKAGING (<span style="color&#58;#0c5460;font-weight&#58;normal;"> 3 Business Days </span>)</li></ol></div></div>')
GO
INSERT [dbo].[LetterType] ([Id], [Name], [Description], [IsDeleted], [CreatedBy], [ModifiedBy], [DeletedBy], [CreatedTime], [DeletedTime], [ModifiedTime], [HtmlContent]) VALUES (2, N'ADA Report', NULL, 0, N'CATS Admin', N'CATS Admin', N'CATS Admin', CAST(N'2020-06-23T18:44:12.2466667' AS DateTime2), NULL, CAST(N'2020-06-23T18:44:12.2466667' AS DateTime2), N'<div class="col-xl-12" id="ncwc-modal-lw-rrv-DivID5">
   <div class="col-xl-12" style="margin-bottom&#58;20px;"> 
      <span>Based on your document type ( 
         <span style="color&#58;#0c5460;font-weight&#58;normal;">ADA Report</span> ), your document review process will go through rounds of reviews in the following order&#58;</span> </div><div style="margin-bottom&#58;20px;max-width&#58;90%;"><ol><li style="font-weight&#58;bold;border-bottom-width&#58;1px;border-bottom-style&#58;solid;">RED BUILDING (<span style="color&#58;#0c5460;font-weight&#58;normal;"> 3 Business Days </span>)</li><li style="font-weight&#58;bold;border-bottom-width&#58;1px;border-bottom-style&#58;solid;">PAD/POLICY OFFICIALS (<span style="color&#58;#0c5460;font-weight&#58;normal;"> 3 Business Days </span>)</li><li style="font-weight&#58;bold;border-bottom-width&#58;1px;border-bottom-style&#58;solid;">COMMUNICATIONS &amp; LEGISLATIVE AFFAIRS (<span style="color&#58;#0c5460;font-weight&#58;normal;"> 3 Business Days </span>)</li><li style="font-weight&#58;bold;border-bottom-width&#58;1px;border-bottom-style&#58;solid;">FINAL PACKAGING (<span style="color&#58;#0c5460;font-weight&#58;normal;"> 3 Business Days </span>)</li></ol></div></div>')
GO
INSERT [dbo].[LetterType] ([Id], [Name], [Description], [IsDeleted], [CreatedBy], [ModifiedBy], [DeletedBy], [CreatedTime], [DeletedTime], [ModifiedTime], [HtmlContent]) VALUES (3, N'Congressional Report', NULL, 0, N'CATS Admin', N'CATS Admin', N'CATS Admin', CAST(N'2020-06-23T18:44:12.2466667' AS DateTime2), NULL, CAST(N'2020-06-23T18:44:12.2466667' AS DateTime2), N'<div class="col-xl-12" id="ncwc-modal-lw-rrv-DivID3">
   <div class="col-xl-12" style="margin-bottom&#58;20px;"> 
      <span>Based on your document type ( 
         <span style="color&#58;#0c5460;font-weight&#58;normal;">Congressional Report</span> ), your document review process will go through rounds of reviews in the following order&#58;</span> </div><div style="margin-bottom&#58;20px;max-width&#58;90%;"><ol><li style="font-weight&#58;bold;border-bottom-width&#58;1px;border-bottom-style&#58;solid;">RED BUILDING</li><li style="font-weight&#58;bold;border-bottom-width&#58;1px;border-bottom-style&#58;solid;">RED BUILDING GENERAL COUNSEL</li><li style="font-weight&#58;bold;border-bottom-width&#58;1px;border-bottom-style&#58;solid;">PAD/POLICY OFFICIALS</li><li style="font-weight&#58;bold;border-bottom-width&#58;1px;border-bottom-style&#58;solid;">COMMUNICATIONS &amp; LEGISLATIVE AFFAIRS</li><li style="font-weight&#58;bold;border-bottom-width&#58;1px;border-bottom-style&#58;solid;">GRAY BUILDING GENERAL COUNSEL</li><li style="font-weight&#58;bold;border-bottom-width&#58;1px;border-bottom-style&#58;solid;">FINAL PACKAGING</li></ol></div><div><ul><li>Less than 50 pages - 
            <span style="color&#58;#0c5460;font-weight&#58;normal;">3 Business Days</span> (each round)</li><li>51-100 pages - 
            <span style="color&#58;#0c5460;font-weight&#58;normal;">4 Business Days</span> (each round)</li><li>Over 100 pages - 
            <span style="color&#58;#0c5460;font-weight&#58;normal;">5 Business Days</span> (each round)</li></ul></div></div>')
GO
INSERT [dbo].[LetterType] ([Id], [Name], [Description], [IsDeleted], [CreatedBy], [ModifiedBy], [DeletedBy], [CreatedTime], [DeletedTime], [ModifiedTime], [HtmlContent]) VALUES (4, N'Correspondence', NULL, 0, N'CATS Admin', N'CATS Admin', N'CATS Admin', CAST(N'2020-06-23T18:44:12.2466667' AS DateTime2), NULL, CAST(N'2020-06-23T18:44:12.2466667' AS DateTime2), N'<div class="col-xl-12" id="ncwc-modal-lw-rrv-DivID1">
   <div class="col-xl-12" style="margin-bottom&#58;20px;"> 
      <span>Based on your document type ( 
         <span style="color&#58;#0c5460;font-weight&#58;normal;">Correspondence</span> ), your document review process will go through rounds of reviews in the following order&#58;</span> </div><div style="margin-bottom&#58;20px;max-width&#58;90%;"><ol><li style="font-weight&#58;bold;border-bottom-width&#58;1px;border-bottom-style&#58;solid;">PAD/POLICY OFFICIALS (<span style="color&#58;#0c5460;font-weight&#58;normal;"> 2 Business Days </span>)</li><li style="font-weight&#58;bold;border-bottom-width&#58;1px;border-bottom-style&#58;solid;">LEGISLATIVE AFFAIRS (<span style="color&#58;#0c5460;font-weight&#58;normal;"> 3 Business Days </span>)</li><li style="font-weight&#58;bold;border-bottom-width&#58;1px;border-bottom-style&#58;solid;">GRAY BUILDING GENERAL COUNSEL (<span style="color&#58;#0c5460;font-weight&#58;normal;"> 3 Business Days </span>)</li><li style="font-weight&#58;bold;border-bottom-width&#58;1px;border-bottom-style&#58;solid;">FINAL PACKAGING (<span style="color&#58;#0c5460;font-weight&#58;normal;"> 3 Business Days </span>)</li></ol></div></div>')
GO
INSERT [dbo].[LetterType] ([Id], [Name], [Description], [IsDeleted], [CreatedBy], [ModifiedBy], [DeletedBy], [CreatedTime], [DeletedTime], [ModifiedTime], [HtmlContent]) VALUES (5, N'Memoranda, Guidance, Other Reports', NULL, 0, N'CATS Admin', N'CATS Admin', N'CATS Admin', CAST(N'2020-06-23T18:44:12.2466667' AS DateTime2), NULL, CAST(N'2020-06-23T18:44:12.2466667' AS DateTime2), N'<div class="col-xl-12" id="ncwc-modal-lw-rrv-DivID2">
   <div class="col-xl-12" style="margin-bottom&#58;20px;"> 
      <span>Based on your document type ( 
         <span style="color&#58;#0c5460;font-weight&#58;normal;">Memoranda, Guidance, Other Reports</span> ), your document review process will go through rounds of reviews in the following order&#58;</span> </div><div style="margin-bottom&#58;20px;max-width&#58;90%;"><ol><li style="font-weight&#58;bold;border-bottom-width&#58;1px;border-bottom-style&#58;solid;">RED BUILDING</li><li style="font-weight&#58;bold;border-bottom-width&#58;1px;border-bottom-style&#58;solid;">RED BUILDING GENERAL COUNSEL</li><li style="font-weight&#58;bold;border-bottom-width&#58;1px;border-bottom-style&#58;solid;">PAD/POLICY OFFICIALS</li><li style="font-weight&#58;bold;border-bottom-width&#58;1px;border-bottom-style&#58;solid;">COMMUNICATIONS &amp; LEGISLATIVE AFFAIRS</li><li style="font-weight&#58;bold;border-bottom-width&#58;1px;border-bottom-style&#58;solid;">GRAY BUILDING GENERAL COUNSEL</li><li style="font-weight&#58;bold;border-bottom-width&#58;1px;border-bottom-style&#58;solid;">FINAL PACKAGING</li></ol></div><div><ul><li>Less than 50 pages - 
            <span style="color&#58;#0c5460;font-weight&#58;normal;">3 Business Days</span> (each round)</li><li>51-100 pages - 
            <span style="color&#58;#0c5460;font-weight&#58;normal;">4 Business Days</span> (each round)</li><li>Over 100 pages - 
            <span style="color&#58;#0c5460;font-weight&#58;normal;">5 Business Days</span> (each round)</li></ul></div></div>')
GO
INSERT [dbo].[LetterType] ([Id], [Name], [Description], [IsDeleted], [CreatedBy], [ModifiedBy], [DeletedBy], [CreatedTime], [DeletedTime], [ModifiedTime], [HtmlContent]) VALUES (6, N'Other', NULL, 0, N'CATS Admin', N'CATS Admin', N'CATS Admin', CAST(N'2020-06-23T18:44:12.2466667' AS DateTime2), NULL, CAST(N'2020-06-23T18:44:12.2466667' AS DateTime2), N'<div class="col-xl-12" id="ncwc-modal-lw-rrv-DivID6">
   <div class="col-xl-12" style="margin-bottom&#58;20px;">
      <span>Based on your document type ( 
         <span style="color&#58;#0c5460;font-weight&#58;normal;">Other</span> ), your document review process will go through rounds of reviews in the following order&#58;</span> </div><div style="margin-bottom&#58;20px;max-width&#58;90%;"><ol><li style="font-weight&#58;bold;border-bottom-width&#58;1px;border-bottom-style&#58;solid;">RED BUILDING</li><li style="font-weight&#58;bold;border-bottom-width&#58;1px;border-bottom-style&#58;solid;">RED BUILDING GENERAL COUNSEL</li><li style="font-weight&#58;bold;border-bottom-width&#58;1px;border-bottom-style&#58;solid;">PAD/POLICY OFFICIALS</li><li style="font-weight&#58;bold;border-bottom-width&#58;1px;border-bottom-style&#58;solid;">COMMUNICATIONS &amp; LEGISLATIVE AFFAIRS</li><li style="font-weight&#58;bold;border-bottom-width&#58;1px;border-bottom-style&#58;solid;">GRAY BUILDING GENERAL COUNSEL</li><li style="font-weight&#58;bold;border-bottom-width&#58;1px;border-bottom-style&#58;solid;">FINAL PACKAGING</li></ol></div><div><ul><li>Less than 50 pages - 
            <span style="color&#58;#0c5460;font-weight&#58;normal;">3 Business Days</span> (each round)</li><li>51-100 pages - 
            <span style="color&#58;#0c5460;font-weight&#58;normal;">4 Business Days</span> (each round)</li><li>Over 100 pages - 
            <span style="color&#58;#0c5460;font-weight&#58;normal;">5 Business Days</span> (each round)</li></ul></div></div>')
GO
INSERT [dbo].[LetterType] ([Id], [Name], [Description], [IsDeleted], [CreatedBy], [ModifiedBy], [DeletedBy], [CreatedTime], [DeletedTime], [ModifiedTime], [HtmlContent]) VALUES (7, N'Executive Order', NULL, 0, N'CATS Admin', N'CATS Admin', N'CATS Admin', CAST(N'2021-03-11T09:43:47.5133333' AS DateTime2), NULL, CAST(N'2021-03-11T09:43:47.5133333' AS DateTime2), NULL)
GO
SET IDENTITY_INSERT [dbo].[LetterType] OFF
GO
INSERT [dbo].[Role] ([Id], [Name], [Description], [CreatedBy], [ModifiedBy], [DeletedBy], [CreatedTime], [ModifiedTime], [DeletedTime], [IsDeleted]) VALUES (1, N'Administrator', N'Manage all the CATS Ressources', N'CATS Admin', NULL, NULL, CAST(N'2020-08-04T09:50:42.4200000' AS DateTime2), CAST(N'2020-08-04T09:50:42.4200000' AS DateTime2), NULL, 0)
GO
INSERT [dbo].[Role] ([Id], [Name], [Description], [CreatedBy], [ModifiedBy], [DeletedBy], [CreatedTime], [ModifiedTime], [DeletedTime], [IsDeleted]) VALUES (2, N'Support', N'Troubleshoot CATS', N'CATS Admin', NULL, NULL, CAST(N'2020-08-04T09:52:49.8566667' AS DateTime2), CAST(N'2020-08-04T09:52:49.8566667' AS DateTime2), NULL, 0)
GO
INSERT [dbo].[Role] ([Id], [Name], [Description], [CreatedBy], [ModifiedBy], [DeletedBy], [CreatedTime], [ModifiedTime], [DeletedTime], [IsDeleted]) VALUES (3, N'Correspondence Team', N'Responsable of creating packages', N'CATS Admin', NULL, NULL, CAST(N'2020-08-04T09:53:57.9400000' AS DateTime2), CAST(N'2020-08-04T09:53:57.9400000' AS DateTime2), NULL, 0)
GO
INSERT [dbo].[Role] ([Id], [Name], [Description], [CreatedBy], [ModifiedBy], [DeletedBy], [CreatedTime], [ModifiedTime], [DeletedTime], [IsDeleted]) VALUES (4, N'Originator', N'Manage Reviews', N'CATS Admin', NULL, NULL, CAST(N'2020-08-04T09:54:38.5900000' AS DateTime2), CAST(N'2020-08-04T09:54:38.5900000' AS DateTime2), NULL, 0)
GO
INSERT [dbo].[Role] ([Id], [Name], [Description], [CreatedBy], [ModifiedBy], [DeletedBy], [CreatedTime], [ModifiedTime], [DeletedTime], [IsDeleted]) VALUES (5, N'Reviewer', N'Review packages', N'CATS Admin', NULL, NULL, CAST(N'2020-08-04T09:55:05.5133333' AS DateTime2), CAST(N'2020-08-04T09:55:05.5133333' AS DateTime2), NULL, 0)
GO
INSERT [dbo].[Role] ([Id], [Name], [Description], [CreatedBy], [ModifiedBy], [DeletedBy], [CreatedTime], [ModifiedTime], [DeletedTime], [IsDeleted]) VALUES (6, N'Originator Surrogate', N'Acting on the behalf of an Originator', N'CATS Admin', NULL, NULL, CAST(N'2020-08-04T09:56:21.0666667' AS DateTime2), CAST(N'2020-08-04T09:56:21.0666667' AS DateTime2), NULL, 0)
GO
INSERT [dbo].[Role] ([Id], [Name], [Description], [CreatedBy], [ModifiedBy], [DeletedBy], [CreatedTime], [ModifiedTime], [DeletedTime], [IsDeleted]) VALUES (7, N'Reviewer Surrogate', N'Acting on behalf of a Reviewer', N'CATS Admin', NULL, NULL, CAST(N'2020-08-04T09:56:54.5133333' AS DateTime2), CAST(N'2020-08-04T09:56:54.5133333' AS DateTime2), NULL, 0)
GO
INSERT [dbo].[Role] ([Id], [Name], [Description], [CreatedBy], [ModifiedBy], [DeletedBy], [CreatedTime], [ModifiedTime], [DeletedTime], [IsDeleted]) VALUES (8, N'Office Manager', N'Super Originator', N'CATS Admin', NULL, NULL, CAST(N'2020-08-04T10:23:56.0200000' AS DateTime2), CAST(N'2020-08-04T10:23:56.0200000' AS DateTime2), NULL, 0)
GO
INSERT [dbo].[Role] ([Id], [Name], [Description], [CreatedBy], [ModifiedBy], [DeletedBy], [CreatedTime], [ModifiedTime], [DeletedTime], [IsDeleted]) VALUES (9, N'Report Manager', N'Can Run Reports', N'CATS Admin', NULL, NULL, CAST(N'2020-08-04T17:51:57.2500000' AS DateTime2), CAST(N'2020-08-04T17:51:57.2500000' AS DateTime2), NULL, 0)
GO
INSERT [dbo].[Role] ([Id], [Name], [Description], [CreatedBy], [ModifiedBy], [DeletedBy], [CreatedTime], [ModifiedTime], [DeletedTime], [IsDeleted]) VALUES (10, N'Correspondence Read Only', N'Can view only the Correspndence Dashboard', N'CATS Admin', NULL, NULL, CAST(N'2020-08-05T08:55:18.1833333' AS DateTime2), CAST(N'2020-08-05T08:55:18.1833333' AS DateTime2), NULL, 0)
GO
INSERT [dbo].[Role] ([Id], [Name], [Description], [CreatedBy], [ModifiedBy], [DeletedBy], [CreatedTime], [ModifiedTime], [DeletedTime], [IsDeleted]) VALUES (11, N'Originator Read Only', N'Can view only the Originator Dashboard', N'CATS Admin', NULL, NULL, CAST(N'2020-08-05T08:56:07.6000000' AS DateTime2), CAST(N'2020-08-05T08:56:07.6000000' AS DateTime2), NULL, 0)
GO
INSERT [dbo].[Role] ([Id], [Name], [Description], [CreatedBy], [ModifiedBy], [DeletedBy], [CreatedTime], [ModifiedTime], [DeletedTime], [IsDeleted]) VALUES (12, N'Copied User', N'Can View Office pending letters', N'CATS Admin', NULL, NULL, CAST(N'2020-08-05T09:00:25.9166667' AS DateTime2), CAST(N'2020-08-05T09:00:25.9166667' AS DateTime2), NULL, 0)
GO
INSERT [dbo].[Role] ([Id], [Name], [Description], [CreatedBy], [ModifiedBy], [DeletedBy], [CreatedTime], [ModifiedTime], [DeletedTime], [IsDeleted]) VALUES (13, N'FYI User', N'Can View assigned review items', N'CATS Admin', NULL, NULL, CAST(N'2020-08-05T09:01:24.7700000' AS DateTime2), CAST(N'2020-08-05T09:01:24.7700000' AS DateTime2), NULL, 0)
GO
ALTER TABLE [dbo].[DLGroup] ADD  CONSTRAINT [DF_DLGroup_ModifiedTime]  DEFAULT (getdate()) FOR [ModifiedTime]
GO
ALTER TABLE [dbo].[DLGroup] ADD  CONSTRAINT [DF_DLGroup_CreatedTime]  DEFAULT (getdate()) FOR [CreatedTime]
GO
ALTER TABLE [dbo].[DLGroup] ADD  CONSTRAINT [DF_DLGroup_CreatedBy]  DEFAULT ('CATS Admin') FOR [CreatedBy]
GO
ALTER TABLE [dbo].[DLGroup] ADD  CONSTRAINT [DF_DLGroup_ModifiedBy]  DEFAULT ('CATS Admin') FOR [ModifiedBy]
GO
ALTER TABLE [dbo].[DLGroup] ADD  CONSTRAINT [DF_DLGroup_IsDeleted]  DEFAULT ((0)) FOR [IsDeleted]
GO
ALTER TABLE [dbo].[DocumentType] ADD  CONSTRAINT [DF_DocumentType_CreatedTime]  DEFAULT (getdate()) FOR [CreatedTime]
GO
ALTER TABLE [dbo].[DocumentType] ADD  CONSTRAINT [DF_DocumentType_ModifiedTime]  DEFAULT (getdate()) FOR [ModifiedTime]
GO
ALTER TABLE [dbo].[DocumentType] ADD  CONSTRAINT [DF_DocumentType_CreatedBy]  DEFAULT (N'CATS Admin') FOR [CreatedBy]
GO
ALTER TABLE [dbo].[DocumentType] ADD  CONSTRAINT [DF_DocumentType_ModifiedBy]  DEFAULT (N'CATS Admin') FOR [ModifiedBy]
GO
ALTER TABLE [dbo].[DocumentType] ADD  CONSTRAINT [DF_DocumentType_IsDeleted]  DEFAULT ((0)) FOR [IsDeleted]
GO
ALTER TABLE [dbo].[LeadOffice] ADD  CONSTRAINT [DF_LeadOffice_IsDeleted]  DEFAULT ((0)) FOR [IsDeleted]
GO
ALTER TABLE [dbo].[LeadOffice] ADD  CONSTRAINT [DF_LeadOffice_CreatedBy]  DEFAULT (N'CATS Admin') FOR [CreatedBy]
GO
ALTER TABLE [dbo].[LeadOffice] ADD  CONSTRAINT [DF_LeadOffice_ModifiedBy]  DEFAULT (N'CATS Admin') FOR [ModifiedBy]
GO
ALTER TABLE [dbo].[LeadOffice] ADD  CONSTRAINT [DF_LeadOffice_DeletedBy]  DEFAULT (N'CATS Admin') FOR [DeletedBy]
GO
ALTER TABLE [dbo].[LeadOffice] ADD  CONSTRAINT [DF_LeadOffice_CreatedTime]  DEFAULT (getdate()) FOR [CreatedTime]
GO
ALTER TABLE [dbo].[LeadOffice] ADD  CONSTRAINT [DF_LeadOffice_ModifiedTime]  DEFAULT (getdate()) FOR [ModifiedTime]
GO
ALTER TABLE [dbo].[LeadOffice] ADD  CONSTRAINT [DF_LeadOffice_IsHidden]  DEFAULT ((0)) FOR [IsHidden]
GO
ALTER TABLE [dbo].[LetterType] ADD  CONSTRAINT [DF_LetterType_IsDeleted]  DEFAULT ((0)) FOR [IsDeleted]
GO
ALTER TABLE [dbo].[LetterType] ADD  CONSTRAINT [DF_LetterType_CreatedBy]  DEFAULT (N'CATS Admin') FOR [CreatedBy]
GO
ALTER TABLE [dbo].[LetterType] ADD  CONSTRAINT [DF_LetterType_ModifiedBy]  DEFAULT (N'CATS Admin') FOR [ModifiedBy]
GO
ALTER TABLE [dbo].[LetterType] ADD  CONSTRAINT [DF_LetterType_DeletedBy]  DEFAULT (N'CATS Admin') FOR [DeletedBy]
GO
ALTER TABLE [dbo].[LetterType] ADD  CONSTRAINT [DF_LetterType_CreatedTime]  DEFAULT (getdate()) FOR [CreatedTime]
GO
ALTER TABLE [dbo].[LetterType] ADD  CONSTRAINT [DF_LetterType_ModifiedTime]  DEFAULT (getdate()) FOR [ModifiedTime]
GO
ALTER TABLE [dbo].[Role] ADD  CONSTRAINT [DF_Roles_CreatedBy]  DEFAULT (N'CATS Admin') FOR [CreatedBy]
GO
ALTER TABLE [dbo].[Role] ADD  CONSTRAINT [DF_Roles_CreatedTime]  DEFAULT (getdate()) FOR [CreatedTime]
GO
ALTER TABLE [dbo].[Role] ADD  CONSTRAINT [DF_Roles_ModifiedTime]  DEFAULT (getdate()) FOR [ModifiedTime]
GO
ALTER TABLE [dbo].[Role] ADD  CONSTRAINT [DF_Role_IsDeleted]  DEFAULT ((0)) FOR [IsDeleted]
GO

/****** Object:  StoredProcedure [dbo].[sp_report_Get_SSRS_Parameter_Sources]    Script Date: 4/30/2021 10:37:07 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		Jean Pita Diomi Kazadi
-- Create date: 11/09/2020
-- Description:	CATS Report details
-- =============================================
CREATE PROCEDURE [dbo].[sp_report_Get_SSRS_Parameter_Sources]  (@letterStatus varchar(100),@startDate datetime2(7), @endDate datetime2(7), @correspondentName varchar(500), @adminsitrationId  varchar(10) = '' )
AS
BEGIN
	select * from [dbo].[ufn_report_Get_SSRS_Parameter_Sources](@letterStatus, @startDate, @endDate, trim(@correspondentName), @adminsitrationId)
END

--exec  [dbo].[sp_report_Get_SSRS_Parameter_Sources] @letterStatus= '', @startDate = '01/01/1900', @endDate = '12/2/2019'
GO


/****** Object:  StoredProcedure [dbo].[sp_report_Open_Status_Details]    Script Date: 4/30/2021 9:57:21 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Jean Pita Diomi Kazadi
-- Create date: 11/09/2020
-- Description:	CATS Report details
-- =============================================
CREATE PROCEDURE [dbo].[sp_report_Open_Status_Details]  (@startDate datetime2(7), @endDate datetime2(7), @correspondentName varchar(500), @isUnderReviewOnly bit = 0, @adminsitrationId varchar(10) = '' )
AS
BEGIN
	select [CATSID], [CorrespondentName], [CurrentReview], [LeadOfficeName], [LetterTypeName], [CreatedBy],ModifiedBy, format(cast(CreatedTime as date) , 'MM/dd/yyyy') CreatedTime, format(cast(ModifiedTime as date), 'MM/dd/yyyy') ModifiedTime, 
		[LetterSubject],[LetterStatus],[ReviewStatus],[IsUnderReview],
		[dbo].[ufn_report_Open_Status_Details_Action_Date]([CATSID],[CurrentReview]) as actionDate, 
		(SELECT roundstartdate FROM  [ufn_report_Open_Status_Details_RoundStartEnd_Date]([CATSID],[CurrentReview])) as roundStartDate,
		(SELECT roundenddate FROM  [ufn_report_Open_Status_Details_RoundStartEnd_Date]([CATSID],[CurrentReview])) as roundEndDate,
		(SELECT TOP 1 CompletedRounds FROM Collaboration WHERE CorrespondenceId = A.Id) as completedRounds, 
		(SELECT daysPastdue FROM  [dbo].[ufn_report_Open_Status_Details_DaysCount_By_Date]([CATSID],[CurrentReview])) as numdaysPastDue,
		(SELECT daysInRound FROM  [dbo].[ufn_report_Open_Status_Details_DaysCount_By_Date]([CATSID],[CurrentReview])) as numdaysInRound,
		(SELECT DaysInSystem FROM  [dbo].[ufn_report_Open_Status_Details_DaysCount_By_Date]([CATSID],[CurrentReview])) as numdaysInSystem
		
		from [dbo].[Correspondence] A -- where IsDeleted = 0;
		where CONVERT(varchar(20), coalesce(AdministrationId,'')) LIKE '%' + @adminsitrationId + '%' and a.LetterStatus like '%open%' and coalesce(CorrespondentName, '') like '%' + trim(@correspondentName) + '%'  and a.IsDeleted != 1 and CreatedTime between @startDate and @endDate-- and a.ReviewStatus like '%' + @reviewStatus + '%'
		AND ((@isUnderReviewOnly = 0 AND a.ReviewStatus like '%%') OR (@isUnderReviewOnly != 0 AND a.IsUnderReview = @isUnderReviewOnly))
		order by [CATSID] desc
END

--exec [dbo].[sp_report_Open_Status_Details] @startDate = '01/01/1900', @endDate = '03/9/2021',@correspondentName='',  @isUnderReviewOnly=1
--exec [dbo].[sp_report_Open_Status_Details] @startDate = '01/01/1900', @endDate = '03/9/2021',@correspondentName='',  @isUnderReviewOnly=0
GO
/****** Object:  StoredProcedure [dbo].[sp_report_Open_Status_Details_By_Reviewer]    Script Date: 4/30/2021 9:57:21 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Jean Pita Diomi Kazadi
-- Create date: 11/09/2020
-- Modified by Jean Pita on 11/27/2020
-- Description:	CATS Report details
-- =============================================
--exec [dbo].[sp_report_Open_Status_Details_By_Reviewer] @CATSID ='2020-CATS-6500', @startDate = '01/01/1900', @endDate = '12/2/2019'
CREATE PROCEDURE [dbo].[sp_report_Open_Status_Details_By_Reviewer]  (@CATSID varchar(100), @startDate datetime2(7) = null, @endDate datetime2(7) = null)
AS
BEGIN

SELECT CATSID,  RoundName, ReviewerName, RoundCompletedBy, Status, LeadOfficeName, CorrespondentName, LetterSubject, LetterType, 
format(cast(RevCreated as date), 'MM/dd/yyyy') RevCreated, 
format(cast(RevModifiedTime as date), 'MM/dd/yyyy') RevModifiedTime, 
format(cast(CreatedTime as date), 'MM/dd/yyyy') CreatedTime, 
format(cast(RoundStartDate as date), 'MM/dd/yyyy') RoundStartDate, 
format(cast(RoundEndDate as date), 'MM/dd/yyyy') RoundEndDate, 
format(cast(ActionDate as date), 'MM/dd/yyyy') ActionDate, 
format(cast(LastUpdateDate as date), 'MM/dd/yyyy') LastUpdateDate, 
format(cast(LastOpenDate as date), 'MM/dd/yyyy') LastOpenDate, 
DaysInSystem,
daysPastdue, 
daysInRound

from (

	select 
	a.[CATSID], 
	a.RoundName, 
	a.ReviewerName,
	a.RoundCompletedBy,
	a.Status,
	(select LeadOfficeName from [dbo].[Correspondence] where CATSID = a.[CATSID]) LeadOfficeName,	
	(select CorrespondentName from [dbo].[Correspondence] where CATSID = a.[CATSID]) CorrespondentName,	
	(select LetterSubject from [dbo].[Correspondence] where CATSID = a.[CATSID]) LetterSubject,	
	(select LetterTypeName from [dbo].[Correspondence] where CATSID = a.[CATSID]) LetterType,
	min(a.RevCreated) RevCreated,-- review assigned to the reviewer created date
	max(a.RevModifiedTime) RevModifiedTime, -- review modified by the reviewer 
	min(a.[CreatedTime]) CreatedTime, -- Correspondence created date
	min(a.[RoundStartDate]) RoundStartDate, 
	max(a.[RoundEndDate]) RoundEndDate,
	max(a.ActionDate) ActionDate,-- date the review round has been either completed or draft by the reviewer
	max(a.LastUpdateDate) LastUpdateDate , -- last date the round has been completed
	max(a.LastOpenDate) LastOpenDate,-- last day the correspondence has been open
	case
		when ReviewerName like '%DL-%' then abs([dbo].[ufn_report_statistics_Calculate_numOfDays_Exclude_WkEnd]( min(a.LastUpdateDate), max(a.[RoundEndDate])))
		when [dbo].[ufn_report_statistics_Calculate_numOfDays_Exclude_WkEnd]((case when max(a.ActionDate)  is null then a.LastOpenDate else a.LastUpdateDate end), max(a.[RoundEndDate])) < 0 then abs([dbo].[ufn_report_statistics_Calculate_numOfDays_Exclude_WkEnd]((case when max(a.ActionDate)  is null then a.LastOpenDate else a.LastUpdateDate end), max(a.[RoundEndDate])))
		else
			0	
	end daysPastdue,
	case 
		when ReviewerName like '%DL-%' then abs([dbo].[ufn_report_statistics_Calculate_numOfDays_Exclude_WkEnd]( min(a.RevCreated), max(a.LastUpdateDate)))
		when [dbo].[ufn_report_statistics_Calculate_numOfDays_Exclude_WkEnd](min(a.RevCreated),(case when max(a.ActionDate)  is null then a.LastOpenDate else a.LastUpdateDate end)) < 0 then 0
		else [dbo].[ufn_report_statistics_Calculate_numOfDays_Exclude_WkEnd](min(a.RevCreated),(case when max(a.ActionDate)  is null then a.LastOpenDate else a.LastUpdateDate end))
	end daysInRound,
	--[dbo].[ufn_report_statistics_Calculate_numOfDays_Exclude_WkEnd](max(a.ActionDate), max(a.[RoundEndDate])) daysPastdue,
	[dbo].[ufn_report_statistics_Calculate_numOfDays_Exclude_WkEnd](min(a.[CreatedTime]), a.LastOpenDate) DaysInSystem
from 
		(
			select 
				Cor.[CATSID], 
				rev.ReviewerName,
				rev.ReviewerUPN,
				rev.RoundName, 
				rev.RoundCompletedDate,
				rev.DraftDate,
				rev.[CreatedTime] AS RevCreated, 
				rev.[ModifiedTime] AS RevModifiedTime,
				Cor.[CreatedTime], 
				Cor.[ModifiedTime], 
				rev.[RoundStartDate], 
				rev.[RoundEndDate], 
				(case 
					when rev.RoundCompletedBy is not null and cor.LetterStatus = 'Closed' then 'Completed Closed'
					when rev.RoundCompletedBy is not null then 'Completed'
					when rev.DraftBy is not null then 'Draft'
					else 'Not Completed' 
				 end) Status,
				(case when rev.RoundCompletedBy is null then '' else rev.RoundCompletedBy end) RoundCompletedBy,
				(case 
					when rev.RoundCompletedDate is not null and rev.RoundCompletedDate != '' then CONVERT(DATETIME, rev.RoundCompletedDate)
					when rev.DraftDate is not null and rev.DraftDate != '' then CONVERT(DATETIME, rev.DraftDate)
					else null 
				 end) ActionDate,
				(case 
					when rev.RoundCompletedDate is not null and rev.RoundCompletedDate != ''  then CONVERT(DATETIME, rev.RoundCompletedDate)
					--when ISDATE(CONVERT(varchar, rev.RoundCompletedDate)) != 1 and cor.LetterStatus != 'Closed'  then Cor.[ModifiedTime]
					else Cor.[ModifiedTime] 
				 end) LastUpdateDate,
				(case 
					when cor.LetterStatus = 'Close' then Cor.[ModifiedTime]
					else GETDATE()
				 end) LastOpenDate
			from [dbo].[Reviewer] as Rev
			inner join [dbo].[Collaboration] as Col
			on Rev.CollaborationId = Col.id
			inner join [dbo].[Correspondence] as Cor
			on col.CorrespondenceId = cor.id
		) as A 
		group by a.[CATSID],  a.RoundName, a.ReviewerName, a.RoundCompletedBy, a.Status, a.ActionDate, a.LastUpdateDate, a.LastOpenDate
		having a.[CATSID] LIKE '%' +  @CATSID + '%' --and a.ReviewerName not like '%DL-%' --and a.RoundName like '%'-- +  @currentRound + '%'--desc

) C

group by CATSID,  RoundName, ReviewerName, RoundCompletedBy, Status, LeadOfficeName, CorrespondentName, LetterSubject, LetterType,
		 RevCreated,RevModifiedTime, CreatedTime, RoundStartDate, RoundEndDate, ActionDate, LastUpdateDate, LastOpenDate, daysPastdue, daysInRound, DaysInSystem

END

--exec [dbo].[sp_report_Open_Status_Details_By_Reviewer] @CATSID ='2018-cats-5199', @startDate = '01/01/1900', @endDate = '12/2/2019'
GO
/****** Object:  StoredProcedure [dbo].[sp_report_statistics]    Script Date: 4/30/2021 9:57:21 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Jean Pita Diomi Kazadi
-- Create date: 11/09/2020
-- Description:	calculat the Number of CATS Open Items
-- =============================================
CREATE PROCEDURE [dbo].[sp_report_statistics](@startDate datetime2(7), @endDate datetime2(7), @correspondentName varchar(500)='', @adminsitrationId varchar(10) = '')
AS
BEGIN
	SELECT * FROM  [dbo].[ufn_report_statistics_numberOfCATS_Open_items](@startDate, @endDate, trim(@correspondentName), @adminsitrationId)
END

--exec [dbo].[sp_report_statistics] '01/01/2018', '02/22/2021', '', ''
GO
/****** Object:  StoredProcedure [dbo].[sp_report_top_Open_The_Longest]    Script Date: 4/30/2021 9:57:21 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Jean Pita Diomi Kazadi
-- Create date: 11/09/2020
-- Description:	returns the top N longest open items
-- =============================================
CREATE PROCEDURE [dbo].[sp_report_top_Open_The_Longest] (@startDate datetime2(7), @endDate datetime2(7), @correspondentName varchar(500), @adminsitrationId varchar(10) = '' )
AS
BEGIN
	
	select top 5 CATSID, [LeadOfficeName],[LetterTypeName],  [dbo].[ufn_report_statistics_Calculate_numOfDays_Exclude_WkEnd](CreatedTime, case when LetterStatus like '%open%' then GetDate() else ModifiedTime end) as DAYSOpen, CreatedTime, ModifiedTime 
	from [dbo].[Correspondence] 
	where CONVERT(varchar(20), AdministrationId) LIKE '%' + @adminsitrationId + '%' AND LetterStatus like '%open%'and IsUnderReview = 1 and  CorrespondentName like '%' + trim(@correspondentName) + '%' and IsDeleted = 0 and CreatedTime between @startDate and @endDate
	
	order by DAYSOpen desc
END

--exec [dbo].[sp_report_top_Open_The_Longest]  '1/1/2018', '2/9/2021','', '3'
GO
