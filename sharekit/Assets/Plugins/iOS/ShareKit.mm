//
//  ShareKit.mm
//  Unity-iPhone
//
//  Created by kuan on 2020/06/03.
//

extern "C" {
    
    #define GetStringParam( _x_ ) ( _x_ != NULL ) ? [NSString stringWithUTF8String:_x_] : [NSString stringWithUTF8String:""]
    
    void ShareKit_Open(const char *text, const char *url, const char *textureUri) {
    
        NSString *shareTextureUri = GetStringParam(textureUri);
        UIImage *shareImg = nil;
        if ([shareTextureUri length] != 0) {
            shareImg = [UIImage imageWithContentsOfFile:shareTextureUri];
        }

        NSArray *items = [NSArray arrayWithObjects:GetStringParam(text), GetStringParam(url), shareImg, nil];
        UIActivityViewController *activityView = [[UIActivityViewController alloc] initWithActivityItems:items applicationActivities: nil];
    
        if(floorf(NSFoundationVersionNumber) > NSFoundationVersionNumber_iOS_7_1) {
            activityView.popoverPresentationController.sourceView = UnityGetGLViewController().view;
        }
    
        [UnityGetGLViewController() presentViewController:activityView animated:YES completion:nil];
    }
}
