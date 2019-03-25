Sitecore Commerce Plugin to extend SellablItem with Parent Categories View
======================================

Out of the Box BizFx displays Category-SellableItem association in Category View. 
And you'd need to paginate through the unsorted Grid to find your sellableItem is in the list and disassociate it.
This plugin extends SellableItem EntityView in Bizfx with ParentCategories ChildView.
This childview would list all SellableItem parent categories and allow to disassociate it from one or many parents.

Sponsor
=======
This plugin was sponsored and created by Xcentium.


How to Install
==============

1-	Copy the plugin to your Sitecore Commerce Engine Solution and add it as a project.  
2-	Add it as a dependency to your Sitecore.Commerce.Engine project.  
3-  Find 'Sitecore.Commerce.Plugin.EntityVersions.EntityVersionsActionsPolicy' in 'PlugIn.Versioning.PolicySet-1.0.0.json' and add      'DisassociateItemFromCategory' to 'AllowedActions'.  
4-  Re-Bootstrap.

How to Use
==============
Go to BizFx(Business Tools) and open any SellableItem. 
You should see a Parent Category child view.

Note:
=====

- If you have any questions, comment or need us to help install, extend or adapt to your needs, do not hesitate to reachout to us at XCentium.
