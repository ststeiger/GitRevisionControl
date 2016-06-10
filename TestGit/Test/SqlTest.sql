exec sp_executesql N'/*
	declare @MDT_ID int; set @MDT_ID = 0;
	declare @BE_ID int; set @BE_ID = 12435;
	declare @OBJ uniqueidentifier; set @OBJ = ''FC8F3193-B573-4F34-88A9-2AA182DA7757'';
	declare @OBJT varchar(10); set @OBJT = ''SO'';
*/

declare @tTemplate nvarchar(500);
set @tTemplate = ''@site?uid=@obj&env=ov&ro=false&proc=@proc&mandant=@mdt'';

select top 1
	replace(
	replace(
	replace(
	replace(@tTemplate,
		''@site'', [OBJT_Kurz_FR]),
		''@obj'', cast(@OBJ as varchar(36))),
		''@proc'', [BE_Hash]),
		''@mdt'', cast(isnull(@MDT_ID, 0) as nvarchar(10))
	) as [OBJ_Link]
from
	[T_OV_Ref_Objekttyp]
	inner join [T_Benutzer] on [BE_ID] = @BE_ID
where
	(
		([OBJT_Status] = 1) and
		([OBJT_Code] = @OBJT)
	)',N'@___ResourceName nvarchar(35),@BE_ID int,@MDT_ID bigint,@OBJ nvarchar(36),@OBJT nvarchar(2)',@___ResourceName=N'.Objectmanagement.OM_Edit_Link2.sql',@BE_ID=0,@MDT_ID=0,@OBJ=N'd18fe1f9-86c9-4629-a1d7-f90d80ef268d',@OBJT=N'RM'
go
