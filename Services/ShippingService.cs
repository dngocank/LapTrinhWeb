using BTL_PTTKYC.Models;

namespace BTL_PTTKYC.Services
{
    public class ShippingService : IShippingService
    {
        private readonly List<DistrictDistanceInfo> _districtDatabase = new()
        {
            // Hà Nội - Nội thành (Kho trung tâm đặt tại 123 Thái Hà, Đống Đa)
            new DistrictDistanceInfo { Province = "Hà Nội", District = "Đống Đa", EstimatedDistanceKm = 1.2m, IsInnerCity = true, Region = "North" },
            new DistrictDistanceInfo { Province = "Hà Nội", District = "Ba Đình", EstimatedDistanceKm = 3.5m, IsInnerCity = true, Region = "North" },
            new DistrictDistanceInfo { Province = "Hà Nội", District = "Cầu Giấy", EstimatedDistanceKm = 3.8m, IsInnerCity = true, Region = "North" },
            new DistrictDistanceInfo { Province = "Hà Nội", District = "Thanh Xuân", EstimatedDistanceKm = 3.2m, IsInnerCity = true, Region = "North" },
            new DistrictDistanceInfo { Province = "Hà Nội", District = "Hoàn Kiếm", EstimatedDistanceKm = 5.2m, IsInnerCity = true, Region = "North" },
            new DistrictDistanceInfo { Province = "Hà Nội", District = "Hai Bà Trưng", EstimatedDistanceKm = 4.8m, IsInnerCity = true, Region = "North" },
            new DistrictDistanceInfo { Province = "Hà Nội", District = "Tây Hồ", EstimatedDistanceKm = 6.5m, IsInnerCity = true, Region = "North" },
            new DistrictDistanceInfo { Province = "Hà Nội", District = "Hoàng Mai", EstimatedDistanceKm = 6.8m, IsInnerCity = true, Region = "North" },
            new DistrictDistanceInfo { Province = "Hà Nội", District = "Hà Đông", EstimatedDistanceKm = 8.5m, IsInnerCity = true, Region = "North" },
            new DistrictDistanceInfo { Province = "Hà Nội", District = "Nam Từ Liêm", EstimatedDistanceKm = 7.2m, IsInnerCity = true, Region = "North" },
            new DistrictDistanceInfo { Province = "Hà Nội", District = "Bắc Từ Liêm", EstimatedDistanceKm = 9.8m, IsInnerCity = true, Region = "North" },
            new DistrictDistanceInfo { Province = "Hà Nội", District = "Long Biên", EstimatedDistanceKm = 10.5m, IsInnerCity = false, Region = "North" },
            // Hà Nội - Ngoại thành
            new DistrictDistanceInfo { Province = "Hà Nội", District = "Gia Lâm", EstimatedDistanceKm = 15.0m, IsInnerCity = false, Region = "North" },
            new DistrictDistanceInfo { Province = "Hà Nội", District = "Thanh Trì", EstimatedDistanceKm = 11.5m, IsInnerCity = false, Region = "North" },
            new DistrictDistanceInfo { Province = "Hà Nội", District = "Hoài Đức", EstimatedDistanceKm = 16.0m, IsInnerCity = false, Region = "North" },
            new DistrictDistanceInfo { Province = "Hà Nội", District = "Đông Anh", EstimatedDistanceKm = 18.5m, IsInnerCity = false, Region = "North" },
            new DistrictDistanceInfo { Province = "Hà Nội", District = "Đan Phượng", EstimatedDistanceKm = 21.0m, IsInnerCity = false, Region = "North" },
            new DistrictDistanceInfo { Province = "Hà Nội", District = "Thường Tín", EstimatedDistanceKm = 24.5m, IsInnerCity = false, Region = "North" },
            new DistrictDistanceInfo { Province = "Hà Nội", District = "Sóc Sơn", EstimatedDistanceKm = 33.0m, IsInnerCity = false, Region = "North" },
            new DistrictDistanceInfo { Province = "Hà Nội", District = "Sơn Tây", EstimatedDistanceKm = 42.0m, IsInnerCity = false, Region = "North" },
            new DistrictDistanceInfo { Province = "Hà Nội", District = "Ba Vì", EstimatedDistanceKm = 56.0m, IsInnerCity = false, Region = "North" },
            // Các tỉnh miền Bắc
            new DistrictDistanceInfo { Province = "Bắc Ninh", District = "TP. Bắc Ninh", EstimatedDistanceKm = 38.0m, IsInnerCity = false, Region = "North" },
            new DistrictDistanceInfo { Province = "Hưng Yên", District = "TP. Hưng Yên", EstimatedDistanceKm = 55.0m, IsInnerCity = false, Region = "North" },
            new DistrictDistanceInfo { Province = "Hải Phòng", District = "Hồng Bàng", EstimatedDistanceKm = 118.0m, IsInnerCity = false, Region = "North" },
            new DistrictDistanceInfo { Province = "Quảng Ninh", District = "Hạ Long", EstimatedDistanceKm = 155.0m, IsInnerCity = false, Region = "North" },
            new DistrictDistanceInfo { Province = "Vĩnh Phúc", District = "Vĩnh Yên", EstimatedDistanceKm = 58.0m, IsInnerCity = false, Region = "North" },
            new DistrictDistanceInfo { Province = "Thái Nguyên", District = "TP. Thái Nguyên", EstimatedDistanceKm = 82.0m, IsInnerCity = false, Region = "North" },
            // Miền Trung
            new DistrictDistanceInfo { Province = "Đà Nẵng", District = "Hải Châu", EstimatedDistanceKm = 760.0m, IsInnerCity = false, Region = "Central" },
            new DistrictDistanceInfo { Province = "Huế", District = "TP. Huế", EstimatedDistanceKm = 660.0m, IsInnerCity = false, Region = "Central" },
            new DistrictDistanceInfo { Province = "Nghệ An", District = "TP. Vinh", EstimatedDistanceKm = 295.0m, IsInnerCity = false, Region = "Central" },
            // Miền Nam
            new DistrictDistanceInfo { Province = "TP. Hồ Chí Minh", District = "Quận 1", EstimatedDistanceKm = 1720.0m, IsInnerCity = false, Region = "South" },
            new DistrictDistanceInfo { Province = "TP. Hồ Chí Minh", District = "Quận Bình Thạnh", EstimatedDistanceKm = 1715.0m, IsInnerCity = false, Region = "South" },
            new DistrictDistanceInfo { Province = "Bình Dương", District = "Thủ Dầu Một", EstimatedDistanceKm = 1690.0m, IsInnerCity = false, Region = "South" },
            new DistrictDistanceInfo { Province = "Đồng Nai", District = "Biên Hòa", EstimatedDistanceKm = 1695.0m, IsInnerCity = false, Region = "South" },
            new DistrictDistanceInfo { Province = "Cần Thơ", District = "Ninh Kiều", EstimatedDistanceKm = 1880.0m, IsInnerCity = false, Region = "South" }
        };

        public DistrictDistanceInfo GetDistrictInfo(string province, string? district)
        {
            if (string.IsNullOrWhiteSpace(province))
            {
                province = "Hà Nội";
            }

            var matched = _districtDatabase.FirstOrDefault(d => 
                d.Province.Equals(province.Trim(), StringComparison.OrdinalIgnoreCase) && 
                (!string.IsNullOrEmpty(district) && d.District.Equals(district.Trim(), StringComparison.OrdinalIgnoreCase)));

            if (matched != null) return matched;

            matched = _districtDatabase.FirstOrDefault(d => d.Province.Equals(province.Trim(), StringComparison.OrdinalIgnoreCase));
            if (matched != null) return matched;

            // Default fallback
            return new DistrictDistanceInfo
            {
                Province = province,
                District = district ?? "Trung tâm",
                EstimatedDistanceKm = province.Contains("Hồ Chí Minh") ? 1720 : (province.Contains("Đà Nẵng") ? 760 : 35),
                IsInnerCity = false,
                Region = province.Contains("Hồ Chí Minh") || province.Contains("Cần Thơ") ? "South" : (province.Contains("Đà Nẵng") ? "Central" : "North")
            };
        }

        public List<DistrictDistanceInfo> GetAllDistricts() => _districtDatabase;

        public decimal EstimateDistance(string province, string? district)
        {
            return GetDistrictInfo(province, district).EstimatedDistanceKm;
        }

        public ShippingQuoteResult CalculateShipping(ShippingQuoteRequest request)
        {
            var result = new ShippingQuoteResult
            {
                Request = request
            };

            var districtInfo = GetDistrictInfo(request.DestinationProvince, request.DestinationDistrict);
            decimal distance = request.DistanceKm > 0 ? request.DistanceKm : districtInfo.EstimatedDistanceKm;
            result.Request.DistanceKm = distance;

            // Tính trọng lượng quy đổi kích thước: V = (Dài x Rộng x Cao) / 5000 (cm sang kg)
            decimal volWeightKg = Math.Round((request.LengthCm * request.WidthCm * request.HeightCm) / 5000m, 2);
            decimal actualWeightKg = Math.Round(request.WeightGrams / 1000m, 2);
            decimal chargeableWeightKg = Math.Max(actualWeightKg, volWeightKg);

            bool isHanoiInner = districtInfo.Province == "Hà Nội" && districtInfo.IsInnerCity && distance <= 12m;
            bool isHanoiOuter = districtInfo.Province == "Hà Nội" && !isHanoiInner && distance <= 35m;
            bool isNorthRegion = districtInfo.Region == "North" && districtInfo.Province != "Hà Nội";
            bool isInterRegion = districtInfo.Region == "Central" || districtInfo.Region == "South";

            result.CalculatedScope = isHanoiInner 
                ? "Nội thành Hà Nội (Bán kính gần, hỗ trợ hỏa tốc xe máy)" 
                : (isHanoiOuter 
                    ? "Ngoại thành Hà Nội" 
                    : (isNorthRegion ? "Liên tỉnh - Miền Bắc" : "Liên miền (Bắc - Trung - Nam)"));

            // Phí đóng gói chuẩn chống sốc điện tử (thùng carton 3-5 lớp, xốp bóng khí 4 vòng, tem vỡ KittyPhone)
            decimal packingFee = 10000m;

            // Phí bảo hiểm hàng công nghệ giá trị cao (High-value Declared Goods): 0.5% giá trị, tối thiểu 10.000đ, tối đa 150.000đ
            // ĐÂY LÀ ĐIỂM CỐT TỬ NGĂN CHẶN LỖ HỔNG: Nếu bưu tá làm mất iPhone 20-30 triệu mà không có bảo hiểm, bưu cục chỉ đền 140.000đ!
            decimal insuranceFee = 0;
            if (request.DeclaredValue > 1000000m)
            {
                insuranceFee = Math.Min(150000m, Math.Max(10000m, Math.Round(request.DeclaredValue * 0.005m / 1000m) * 1000m));
            }

            // Tính toán mức trợ giá của Cửa hàng KittyPhone (Store Subsidy)
            // Nhằm đảm bảo giá khách trả KHÔNG QUÁ ĐẮT, giữ chân khách hàng
            decimal storeSubsidy = 0;
            string subsidyReason = string.Empty;

            if (request.IsWarrantyService)
            {
                if (request.IsUnderWarranty)
                {
                    // Thiết bị còn hạn bảo hành: KittyPhone tài trợ 50% đến 100% cước ship 2 chiều
                    storeSubsidy = 35000m;
                    subsidyReason = "Tài trợ 100% cước tiêu chuẩn bưu cục cho máy còn hạn bảo hành chính hãng";
                }
                else
                {
                    storeSubsidy = 15000m;
                    subsidyReason = "Hỗ trợ 15.000đ phí kiểm tra và vận chuyển cho thiết bị hết hạn bảo hành";
                }
            }
            else
            {
                // Mua hàng: Nếu giá trị đơn >= 5 triệu -> tài trợ tối đa 35.000đ (tương đương miễn phí ship GHN)
                if (request.DeclaredValue >= 5000000m)
                {
                    storeSubsidy = 35000m;
                    subsidyReason = "Freeship: KittyPhone tài trợ 35.000đ cho đơn hàng giá trị trên 5.000.000đ";
                }
                else if (request.DeclaredValue >= 1000000m)
                {
                    storeSubsidy = 20000m;
                    subsidyReason = "KittyPhone hỗ trợ 20.000đ cước vận chuyển đơn hàng";
                }

                // Cộng thêm chiết khấu theo hạng thành viên KittyMember
                if (request.CustomerRank == "Gold")
                {
                    storeSubsidy += 10000m;
                    subsidyReason += " + Thêm 10.000đ đặc quyền hạng Vàng";
                }
                else if (request.CustomerRank == "Diamond")
                {
                    storeSubsidy += 20000m;
                    subsidyReason += " + Thêm 20.000đ đặc quyền hạng Kim Cương";
                }
            }

            var carrierList = new List<ShippingCarrierOption>();

            // 1. XANH SM EXPRESS (Giao xe máy điện VinFast - Siêu tốc 30 - 60 phút)
            bool isXanhSmFeasible = distance <= 25m && districtInfo.Province == "Hà Nội";
            decimal xanhSmBase = 14000m; // 2km đầu
            decimal xanhSmDistance = 0;
            if (distance > 2m)
            {
                if (distance <= 10m)
                {
                    xanhSmDistance = Math.Round((distance - 2m) * 5000m);
                }
                else
                {
                    xanhSmDistance = Math.Round((8m * 5000m) + ((distance - 10m) * 5500m));
                }
            }
            carrierList.Add(new ShippingCarrierOption
            {
                CarrierId = "xanhsm",
                CarrierName = "Xanh SM Express",
                ServiceName = "Giao tức thời xe máy điện VinFast",
                ServiceTypeBadge = "Giao hỏa tốc 30 - 60 phút",
                EstimatedDeliveryTime = "30 - 60 phút",
                BadgeColor = "success",
                LogoUrl = "/images/carriers/xanhsm.png",
                BaseFare = xanhSmBase,
                DistanceFare = xanhSmDistance,
                WeightFare = 0, // Dưới 20kg không phụ thu
                VolumetricWeightKg = volWeightKg,
                ChargeableWeightKg = chargeableWeightKg,
                InsuranceFee = insuranceFee,
                PackagingFee = packingFee,
                StoreSubsidy = storeSubsidy,
                SubsidyDescription = subsidyReason,
                IsFeasible = isXanhSmFeasible,
                InfeasibleReason = isXanhSmFeasible ? null : "Khoảng cách vượt quá bán kính phục vụ xe máy nội thành (tối đa 25km)",
                IsRecommended = isXanhSmFeasible && distance <= 10m,
                Highlights = new List<string>
                {
                    "Xe máy điện 100% êm ái, thân thiện môi trường",
                    "Tài xế nhận hàng ngay sau 5-10 phút",
                    "Theo dõi lộ trình tài xế trực tiếp qua GPS thời gian thực",
                    "Chống sốc tuyệt đối cho điện thoại và máy tính bảng"
                },
                RiskPreventionNote = "Áp dụng bảo hiểm hành trình 100% giá trị máy. Giới hạn bán kính 25km để không đẩy cước vượt kiểm soát."
            });

            // 2. GRABEXPRESS SIÊU TỐC (Grab xe máy - Có mặt ngay trong 45 phút)
            bool isGrabFeasible = distance <= 25m && districtInfo.Province == "Hà Nội";
            decimal grabBase = 16000m; // 2km đầu
            decimal grabDistance = distance > 2m ? Math.Round((distance - 2m) * 5500m) : 0;
            carrierList.Add(new ShippingCarrierOption
            {
                CarrierId = "grab",
                CarrierName = "GrabExpress Siêu Tốc",
                ServiceName = "Giao hàng Grab tức thì nội thành",
                ServiceTypeBadge = "Giao nhanh 45 - 90 phút",
                EstimatedDeliveryTime = "45 - 90 phút",
                BadgeColor = "success",
                LogoUrl = "/images/carriers/grab.png",
                BaseFare = grabBase,
                DistanceFare = grabDistance,
                WeightFare = 0,
                VolumetricWeightKg = volWeightKg,
                ChargeableWeightKg = chargeableWeightKg,
                InsuranceFee = insuranceFee,
                PackagingFee = packingFee,
                StoreSubsidy = storeSubsidy,
                SubsidyDescription = subsidyReason,
                IsFeasible = isGrabFeasible,
                InfeasibleReason = isGrabFeasible ? null : "Vượt quá bán kính giao xe máy GrabExpress (tối đa 25km)",
                IsRecommended = false,
                Highlights = new List<string>
                {
                    "Mạng lưới tài xế Grab đông đảo nhất toàn thành phố",
                    "Giao nhận tận cửa phòng / căn hộ chung cư",
                    "Bảo hiểm an tâm Grab chuyến đi tới 30.000.000đ"
                },
                RiskPreventionNote = "Có tích hợp gói An Tâm Hàng Giá Trị Cao bảo vệ 100% hóa đơn mua/bảo hành."
            });

            // 3. BEDELIVERY TIẾT KIỆM (Be xe máy - Cước tối ưu)
            bool isBeFeasible = distance <= 25m && districtInfo.Province == "Hà Nội";
            decimal beBase = 14500m; // 2km đầu
            decimal beDistance = distance > 2m ? Math.Round((distance - 2m) * 5000m) : 0;
            carrierList.Add(new ShippingCarrierOption
            {
                CarrierId = "be",
                CarrierName = "BeDelivery",
                ServiceName = "Giao hàng Be hỏa tốc nội thành",
                ServiceTypeBadge = "Giao nhanh 1 - 2 giờ",
                EstimatedDeliveryTime = "1 - 2 giờ",
                BadgeColor = "warning",
                LogoUrl = "/images/carriers/be.png",
                BaseFare = beBase,
                DistanceFare = beDistance,
                WeightFare = 0,
                VolumetricWeightKg = volWeightKg,
                ChargeableWeightKg = chargeableWeightKg,
                InsuranceFee = insuranceFee,
                PackagingFee = packingFee,
                StoreSubsidy = storeSubsidy,
                SubsidyDescription = subsidyReason,
                IsFeasible = isBeFeasible,
                InfeasibleReason = isBeFeasible ? null : "Vượt quá cự ly giao tức thời của BeDelivery (tối đa 25km)",
                IsRecommended = false,
                Highlights = new List<string>
                {
                    "Giá cước xe máy cạnh tranh hàng đầu",
                    "Thích hợp các đơn giao trong ngày quanh Hà Nội",
                    "Giao nhận tận nơi, gọi điện trước khi đến"
                },
                RiskPreventionNote = "Tự động kích hoạt gói bảo vệ kiện hàng công nghệ của Be."
            });

            // 4. GIAO HÀNG NHANH (GHN - Bưu cục tiêu chuẩn Toàn quốc)
            decimal ghnBase = isHanoiInner ? 18000m : (isHanoiOuter ? 25000m : (isNorthRegion ? 32000m : 35000m));
            decimal ghnWeightSurcharge = 0;
            if (chargeableWeightKg > 0.5m)
            {
                decimal excessHalfKg = Math.Ceiling((chargeableWeightKg - 0.5m) / 0.5m);
                decimal perHalfKg = isHanoiInner ? 3000m : (isHanoiOuter ? 4000m : 5000m);
                ghnWeightSurcharge = excessHalfKg * perHalfKg;
            }

            carrierList.Add(new ShippingCarrierOption
            {
                CarrierId = "ghn",
                CarrierName = "Giao Hàng Nhanh (GHN)",
                ServiceName = "Chuyển phát nhanh bưu cục GHN",
                ServiceTypeBadge = "Giao chuẩn 1 - 2 ngày",
                EstimatedDeliveryTime = isHanoiInner ? "Trong ngày / 24h" : (isNorthRegion ? "1 - 2 ngày" : "2 - 3 ngày"),
                BadgeColor = "primary",
                LogoUrl = "/images/carriers/ghn.png",
                BaseFare = ghnBase,
                DistanceFare = 0, // GHN tính theo phân vùng
                WeightFare = ghnWeightSurcharge,
                VolumetricWeightKg = volWeightKg,
                ChargeableWeightKg = chargeableWeightKg,
                InsuranceFee = insuranceFee,
                PackagingFee = packingFee,
                StoreSubsidy = storeSubsidy,
                SubsidyDescription = subsidyReason,
                IsFeasible = true,
                InfeasibleReason = null,
                IsRecommended = !isHanoiInner || distance > 10m,
                Highlights = new List<string>
                {
                    "Mạng lưới bưu cục phủ sóng 100% 63 tỉnh thành",
                    "Tính cước chuẩn theo trọng lượng quy đổi kích thước thể tích",
                    "Cước phí cực kỳ tiết kiệm cho tuyến ngoại thành và liên tỉnh",
                    "Hỗ trợ giao lại 3 lần miễn phí nếu người nhận vắng nhà"
                },
                RiskPreventionNote = "Khóa chặt rủi ro mất hàng nhờ quy trình bưu tá quét mã vạch từng trạm và bảo hiểm khai giá 100%."
            });

            // 5. VIETTEL POST (Bưu chính Quân đội - Tiêu chuẩn & Hỏa tốc Bay)
            decimal vtBase = isHanoiInner ? 20000m : (isHanoiOuter ? 26000m : (isNorthRegion ? 32000m : 38000m));
            decimal vtWeightSurcharge = 0;
            if (chargeableWeightKg > 0.5m)
            {
                decimal excessHalfKg = Math.Ceiling((chargeableWeightKg - 0.5m) / 0.5m);
                decimal perHalfKg = isHanoiInner ? 3500m : 5000m;
                vtWeightSurcharge = excessHalfKg * perHalfKg;
            }

            carrierList.Add(new ShippingCarrierOption
            {
                CarrierId = "viettelpost",
                CarrierName = "Viettel Post",
                ServiceName = isInterRegion ? "Viettel Post Hỏa Tốc Bay 24h" : "Viettel Post Chuyển Phát Nhanh",
                ServiceTypeBadge = isInterRegion ? "Hỏa tốc máy bay 12 - 24h" : "Tiêu chuẩn 1 - 2 ngày",
                EstimatedDeliveryTime = isHanoiInner ? "Trong ngày (12h)" : (isNorthRegion ? "24 giờ" : "12 - 24 giờ"),
                BadgeColor = "danger",
                LogoUrl = "/images/carriers/viettelpost.png",
                BaseFare = isInterRegion ? 65000m : vtBase, // Nếu liên miền -> áp dụng gói bay hỏa tốc Viettel Post Fast
                DistanceFare = 0,
                WeightFare = vtWeightSurcharge,
                VolumetricWeightKg = volWeightKg,
                ChargeableWeightKg = chargeableWeightKg,
                InsuranceFee = insuranceFee,
                PackagingFee = packingFee + 3000m, // Đóng thùng carton chuyên nghiệp Viettel Post
                StoreSubsidy = storeSubsidy,
                SubsidyDescription = subsidyReason,
                IsFeasible = true,
                InfeasibleReason = null,
                IsRecommended = isInterRegion,
                Highlights = new List<string>
                {
                    "Bưu chính Quân đội uy tín và bảo mật số 1 Việt Nam",
                    "Đường bay hỏa tốc chuyên tuyến Bắc - Nam kết nối siêu tốc",
                    "Niêm phong chuyên dụng bằng bao bì chống rạch và tem vỡ QR",
                    "Hạn chế tối đa rủi ro thất lạc bưu phẩm công nghệ giá trị cao"
                },
                RiskPreventionNote = "Tiêu chuẩn kiểm soát quân đội nghiêm ngặt nhất, cam kết bồi hoàn 100% theo giá trị khai báo."
            });

            result.AvailableCarriers = carrierList;
            return result;
        }
    }
}
