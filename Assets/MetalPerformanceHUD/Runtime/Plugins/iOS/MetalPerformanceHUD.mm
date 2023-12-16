#import <UIKit/UIKit.h>

// Swift からこちらを取得するのが面倒なので、一旦 ObjC で実装する
extern UIView* UnityGetGLView();

#ifdef __cplusplus
extern "C" {
#endif

void showPerformanceHUD() {
    if (@available(iOS 16.0, *)) {
        auto view = UnityGetGLView();
        ((CAMetalLayer*)(view.layer)).developerHUDProperties =
        @{
            @"mode" : @"default",
            @"logging" : @"default",
        };
    }
}

void hidePerformanceHUD() {
    if (@available(iOS 16.0, *)) {
        auto view = UnityGetGLView();
        ((CAMetalLayer*)(view.layer)).developerHUDProperties = @{};
    }
}

#ifdef __cplusplus
}
#endif
